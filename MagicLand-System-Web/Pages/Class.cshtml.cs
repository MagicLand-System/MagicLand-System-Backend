using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System_Web_Dev.Pages.DataContants;
using MagicLand_System_Web_Dev.Pages.Enums;
using MagicLand_System_Web_Dev.Pages.Helper;
using MagicLand_System_Web_Dev.Pages.Message.SubMessage;
using MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace MagicLand_System_Web_Dev.Pages
{
    public class ClassModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public ClassModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public bool IsLoading { get; set; }

        [BindProperty]
        public List<ClassDefaultMessage> ClassMessages { get; set; } = null;
        [BindProperty]
        public List<CourseWithScheduleShorten> Courses { get; set; } = null;


        public async Task<IActionResult> OnGet()
        {
            IsLoading = false;
            if (ClassMessages == null || ClassMessages.Count == 0)
            {
                var messages = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext!.Session, "DataClass");
                var courses = SessionHelper.GetObjectFromJson<List<CourseWithScheduleShorten>>(HttpContext!.Session, "Courses");

                if (messages != null && messages.Count > 0)
                {
                    ClassMessages = messages;
                }

                if (courses != null && courses.Count > 0)
                {
                    Courses = courses;
                }
                else
                {
                    var result = await _apiHelper.FetchApiAsync<List<CourseWithScheduleShorten>>(ApiEndpointConstant.CourseEndpoint.GetAll, MethodEnum.GET, null);

                    if (result.IsSuccess)
                    {
                        if (result.Data == null)
                        {
                            SessionHelper.SetObjectAsJson(HttpContext.Session, "Courses", Courses);
                        }
                        else
                        {
                            Courses = result.Data;
                            SessionHelper.SetObjectAsJson(HttpContext.Session, "Courses", result.Data!);
                        }

                        return Page();
                    }

                }
            }

            return Page();
        }
        public async Task<IActionResult> OnPostProgressAsync(int inputField, string listCourseId, string submitButton)
        {
            if (submitButton == "Refresh")
            {
                ClassMessages.Clear();

                var result = await _apiHelper.FetchApiAsync<List<CourseWithScheduleShorten>>(ApiEndpointConstant.CourseEndpoint.GetAll, MethodEnum.GET, null);

                if (result.IsSuccess)
                {
                    Courses = result.Data;
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "Courses", result.Data);
                    IsLoading = true;
                    return Page();
                }
            }

            if (inputField == 0 || inputField < 0 || inputField >= 100)
            {
                ViewData["Message"] = "Số Lượng không Hợp Lệ";
                var result = await _apiHelper.FetchApiAsync<List<CourseWithScheduleShorten>>(ApiEndpointConstant.CourseEndpoint.GetAll, MethodEnum.GET, null);

                if (result.IsSuccess)
                {
                    Courses = result.Data;
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "Courses", result.Data);
                    IsLoading = true;
                    return Page();
                }
                return Page();
            }
            ViewData["Message"] = "";

            var courseIdParses = new List<Guid>();
            if (!string.IsNullOrEmpty(listCourseId))
            {
                string pattern = @"\|([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\|";
                MatchCollection matches = Regex.Matches(listCourseId, pattern);

                foreach (Match match in matches)
                {
                    courseIdParses.Add(Guid.Parse(match.Groups[1].Value));
                }
            }

            var courses = SessionHelper.GetObjectFromJson<List<CourseWithScheduleShorten>>(HttpContext!.Session, "Courses");
            if (courseIdParses.Any())
            {
                courses = courseIdParses.Select(id => courses.Single(c => c.CourseId == id)).ToList();
            }
            Random random = new Random();

            foreach (var course in courses)
            {
                for (int order = 0; order < inputField; order++)
                {
                    await RenderProgress(course, order, random);
                }
            }

            SessionHelper.SetObjectAsJson(HttpContext.Session, "DataClass", ClassMessages);
            IsLoading = true;

            return Page();
        }

        private async Task RenderProgress(CourseWithScheduleShorten course, int order, Random random)
        {
            var scheduleRequests = new List<(ScheduleRequest, int)>();
            var scheduleMessages = new List<ScheduleMessage>();
            var startDate = DateTime.Now;

            var lecturer = await GetLecturer(course, random, scheduleRequests, scheduleMessages, startDate);

            var room = random.Next(2, 4) % 2 == 0 ? ClassData.RoomOfflines[random.Next(0, ClassData.RoomOfflines.Count)] : ClassData.RoomOnlines[random.Next(0, ClassData.RoomOnlines.Count)];

            var startDayOfWeek = scheduleRequests.OrderBy(x => x.Item2).First().Item1;

            DayOfWeek targetDayOfWeek;
            if (!Enum.TryParse(startDayOfWeek.DateOfWeek, true, out targetDayOfWeek))
            {
                Console.WriteLine("Invalid day of week string.");
                return;
            }

            while (startDate.DayOfWeek != targetDayOfWeek)
            {
                startDate = startDate.AddDays(1);;
            }

            var objectRequest = new CreateClassRequest
            {
                ClassCode = course.CourseDetail!.SubjectCode + "-" + order,
                CourseId = course.CourseId,
                StartDate = startDate,
                LeastNumberStudent = random.Next(1, 6),
                LimitNumberStudent = random.Next(25, 31),
                LecturerId = lecturer.LectureId,
                Method = random.Next(2, 4) % 2 == 0 ? "OFFLINE" : "ONLINE",
                ScheduleRequests = scheduleRequests.Select(x => x.Item1).ToList(),
                RoomId = Guid.Parse(room.Item2),
            };


            if (lecturer.LectureId == default)
            {
                ClassMessages.Add(new ClassDefaultMessage
                {
                    ClassCode = objectRequest.ClassCode,
                    CourseBeLong = course.CourseDetail!.CourseName!,
                    StartDate = startDate.ToString("MM/dd/yyyy"),
                    LecturerBeLong = "Không",
                    Schedules = scheduleMessages.OrderBy(sc => sc.Order).ToList(),
                    Status = "400",
                    Note = "Không Có Giáo Viên Phù Hợp",
                });

                return;
            }

            var result = await _apiHelper.FetchApiAsync<CreateSingleClassResponse>(ApiEndpointConstant.ClassEnpoint.AddClass, MethodEnum.POST, objectRequest);

            ClassMessages.Add(new ClassDefaultMessage
            {
                ClassCode = objectRequest.ClassCode,
                CourseBeLong = course.CourseDetail!.CourseName!,
                StartDate = startDate.ToString("MM/dd/yyyy"),
                LecturerBeLong = lecturer.FullName!,
                Schedules = scheduleMessages.OrderBy(sc => sc.Order).ToList(),
                Status = result.StatusCode,
                Note = result.Message,
            });


        }

        private async Task<LecturerResponse> GetLecturer(CourseWithScheduleShorten course, Random random, List<(ScheduleRequest, int)> scheduleRequests,
            List<ScheduleMessage> scheduleMessages, DateTime startDate)
        {
            int numberSchedule = random.Next(1, 4);

            for (int i = 0; i < numberSchedule; i++)
            {
                var slot = ClassData.Slots[random.Next(0, ClassData.Slots.Count)];
                var dayOfWeek = ClassData.DayOfWeeks[random.Next(0, ClassData.DayOfWeeks.Count)];


                scheduleRequests.Add((new ScheduleRequest
                {
                    DateOfWeek = dayOfWeek.Item1,
                    SlotId = Guid.Parse(slot.Item1),
                }, dayOfWeek.Item3 == "sunday" ? 8 : int.Parse(dayOfWeek.Item3)));

                scheduleMessages.Add(new ScheduleMessage
                {
                    DayOfWeek = dayOfWeek.Item3,
                    Slot = slot.Item2,
                    Order = dayOfWeek.Item2
                });

            }

            var objectRequest = new FilterLecturerRequest
            {
                StartDate = startDate,
                Schedules = scheduleRequests.Select(x => x.Item1).ToList(),
                CourseId = course.CourseId.ToString(),
            };

            var result = await _apiHelper.FetchApiAsync<List<LecturerResponse>>(ApiEndpointConstant.UserEndpoint.GetLecturer, MethodEnum.POST, objectRequest);
            if (!result.IsSuccess)
            {
                return new LecturerResponse();
            }
            return result.Data[random.Next(0, result.Data.Count)];
        }

        public async Task<IActionResult> OnPostSearchAsync(string searchKey, string searchType)
        {

            if (string.IsNullOrEmpty(searchKey))
            {
                ClassMessages.Clear();

                var result = await _apiHelper.FetchApiAsync<List<CourseWithScheduleShorten>>(ApiEndpointConstant.CourseEndpoint.GetAll, MethodEnum.GET, null);

                if (result.IsSuccess)
                {
                    Courses = result.Data;
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "Courses", result.Data);
                    IsLoading = true;
                    return Page();
                }
            }
            var key = searchKey.Trim().ToLower();
            if (searchType == "MESSAGE")
            {
                var messages = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext!.Session, "DataClass");

                ClassMessages = messages.Where(
                   mess => mess.LecturerPhone.ToLower().Contains(key) ||
                   mess.ClassCode.ToLower().Contains(key) ||
                   mess.CourseBeLong.ToLower().Contains(key)
                   ).ToList();
            }
            if (searchType == "DATA")
            {
                var courses = SessionHelper.GetObjectFromJson<List<CourseWithScheduleShorten>>(HttpContext!.Session, "Courses");

                Courses = courses.Where(
                    c => c.CourseDetail!.CourseName!.ToLower().Contains(key) ||
                    c.CourseDetail.SubjectCode!.ToLower().Contains(key)
                    ).ToList();
            }

            return Page();
        }
    }
}
