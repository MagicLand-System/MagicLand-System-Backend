using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Syllabuses.ForStaff;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System_Web.Pages.DataContants;
using MagicLand_System_Web.Pages.Enums;
using MagicLand_System_Web.Pages.Helper;
using MagicLand_System_Web.Pages.Message;
using MagicLand_System_Web.Pages.Message.SubMessage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MagicLand_System_Web.Pages
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
        public List<ClassMessage> ClassMessages { get; set; } = new List<ClassMessage>();
        [BindProperty]
        public List<CourseWithScheduleShorten> Courses { get; set; } = new List<CourseWithScheduleShorten>();


        public async Task<IActionResult> OnGet()
        {
            IsLoading = false;
            var data = SessionHelper.GetObjectFromJson<List<ClassMessage>>(HttpContext!.Session, "DataClass");
            var courses = SessionHelper.GetObjectFromJson<List<CourseWithScheduleShorten>>(HttpContext!.Session, "Courses");

            if (data != null && data.Count > 0)
            {
                ClassMessages = data;
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
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(int inputField, string listCourseId, string submitButton)
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
            var scheduleRequests = new List<ScheduleRequest>();
            var scheduleMessages = new List<ClassSubMessage>();
            var startDate = DateTime.Now.AddDays(random.Next(1, 4));

            var lecturer = await GetLecturer(course, random, scheduleRequests, scheduleMessages, startDate);

            var room = random.Next(2, 4) % 2 == 0 ? ClassData.RoomOfflines[random.Next(0, ClassData.RoomOfflines.Count)] : ClassData.RoomOnlines[random.Next(0, ClassData.RoomOnlines.Count)];

            var objectRequest = new CreateClassRequest
            {
                ClassCode = course.CourseDetail!.SubjectCode + "-" + order,
                CourseId = course.CourseId,
                StartDate = startDate,
                LeastNumberStudent = random.Next(1, 6),
                LimitNumberStudent = random.Next(25, 31),
                LecturerId = lecturer.LectureId,
                Method = random.Next(2, 4) % 2 == 0 ? "OFFLINE" : "ONLINE",
                ScheduleRequests = scheduleRequests,
                RoomId = Guid.Parse(room.Item2),
            };


            if (lecturer.LectureId == default)
            {
                ClassMessages.Add(new ClassMessage
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

            var result = await _apiHelper.FetchApiAsync<List<LecturerResponse>>(ApiEndpointConstant.UserEndpoint.GetLecturer, MethodEnum.POST, objectRequest);

            ClassMessages.Add(new ClassMessage
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

        private async Task<LecturerResponse> GetLecturer(CourseWithScheduleShorten course, Random random, List<ScheduleRequest> scheduleRequests,
            List<ClassSubMessage> scheduleMessages, DateTime startDate)
        {
            int numberSchedule = random.Next(1, 4);

            for (int i = 0; i < numberSchedule; i++)
            {
                var slot = ClassData.Slots[random.Next(0, ClassData.Slots.Count)];
                var dayOfWeek = ClassData.DayOfWeeks[random.Next(0, ClassData.DayOfWeeks.Count)];

                scheduleRequests.Add(new ScheduleRequest
                {
                    DateOfWeek = dayOfWeek.Item1,
                    SlotId = Guid.Parse(slot.Item1),
                });

                scheduleMessages.Add(new ClassSubMessage
                {
                    DayOfWeek = dayOfWeek.Item1,
                    Slot = slot.Item2,
                    Order = dayOfWeek.Item2
                });

            }

            var objectRequest = new FilterLecturerRequest
            {
                StartDate = startDate,
                Schedules = scheduleRequests,
                CourseId = course.CourseId.ToString(),
            };

            var result = await _apiHelper.FetchApiAsync<List<LecturerResponse>>(ApiEndpointConstant.UserEndpoint.GetLecturer, MethodEnum.POST, objectRequest);
            if (!result.IsSuccess)
            {
                return new LecturerResponse();
            }
            return result.Data[random.Next(0, result.Data.Count)];
        }

    }
}
