using Academic_Blog_App.Services.Helper;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Class;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MagicLand_System_Web.Pages
{
    public class ClassModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private string baseUrl;

        public ClassModel(ILogger<IndexModel> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);
            baseUrl = "http://localhost:5097/api/v1";
            //baseUrl = "https://magiclandapiv2.somee.com/api/v1";
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public void OnGet()
        {

        }
        [BindProperty]
        public List<(string, string, string, string, List<(string, string, int)>, string, string)> messages { get; set; } = default!;
        public async Task<IActionResult> OnPostAsync(int inputField)
        {

            if (inputField == 0 || inputField < 0 || inputField >= 100)
            {
                ViewData["Message"] = "Số Lượng không Hợp Lệ";
                return Page();
            }
            ViewData["Message"] = "";
            var courses = await FetchCourses();

            Random random = new Random();

            string token = SessionHelper.GetObjectFromJson<string>(_httpContextAccessor.HttpContext!.Session, "Token");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            for (int order = 0; order < inputField; order++)
            {
                int courseIndex = random.Next(0, courses!.Count);

                await RenderProgress(courses!.ToList()[courseIndex], order, courseIndex, random);
            }

            return Page();
        }

        private async Task RenderProgress(CourseResExtraInfor course, int order, int courseIndex, Random random)
        {
            var scheduleRequests = new List<ScheduleRequest>();
            var schedules = new List<(string, string, int)>();
            var startDate = DateTime.UtcNow.AddDays(random.Next(1, 4));

            InitData(out List<(string, string)> roomOnline, out List<(string, string)> roomOffline, out List<(string, string)> slots, out List<(string, int)> dayOfWeeks);

            var lecturer = await GetLecturer(course, random, scheduleRequests, schedules, startDate, slots, dayOfWeeks);

            var room = courseIndex % 2 == 0 ? roomOffline[random.Next(0, roomOffline.Count)] : roomOnline[random.Next(0, roomOnline.Count)];
            var createClassRequestData = new CreateClassRequest
            {
                ClassCode = course.CourseDetail!.SubjectCode + "-" + order,
                CourseId = course.CourseId,
                StartDate = startDate,
                LeastNumberStudent = random.Next(3, 8),
                LimitNumberStudent = random.Next(25, 31),
                LecturerId = lecturer.LectureId,
                Method = courseIndex % 2 == 0 ? "OFFLINE" : "ONLINE",
                ScheduleRequests = scheduleRequests,
                RoomId = Guid.Parse(room.Item2),
            };

            if (lecturer.LectureId == default)
            {
                messages.Add(new($"{createClassRequestData.ClassCode}", $"{course.CourseDetail!.CourseName}", $"{startDate.Date}", "Không", schedules.OrderBy(sc => sc.Item3).ToList(), $"{400}", $"Không Có Giáo Viên Phù Hợp"));
                return;
            }

            var jsonContent = new StringContent(JsonSerializer.Serialize(createClassRequestData), Encoding.UTF8, "application/json");
            var insertResponse = await _httpClient.PostAsync(baseUrl + "/classes/add", jsonContent);

            int statusCode = (int)insertResponse.StatusCode;
            string responseMessage = await insertResponse.Content.ReadAsStringAsync();

            messages.Add(new($"{createClassRequestData.ClassCode}", $"{course.CourseDetail!.CourseName}", $"{startDate.ToString("MM/dd/yyyy")}", lecturer.FullName!, schedules.OrderBy(sc => sc.Item3).ToList(), $"{statusCode}", $"{responseMessage}"));
        }

        private async Task<LecturerResponse> GetLecturer(CourseResExtraInfor course, Random random, List<ScheduleRequest> scheduleRequests, List<(string, string, int)> schedules, DateTime startDate, List<(string, string)> slots, List<(string, int)> dayOfWeeks)
        {
            int numberSchedule = random.Next(1, 4);

            for (int i = 0; i < numberSchedule; i++)
            {
                var slot = slots[random.Next(0, slots.Count)];
                var dayOfWeek = dayOfWeeks[random.Next(0, dayOfWeeks.Count)];

                scheduleRequests.Add(new ScheduleRequest
                {
                    DateOfWeek = dayOfWeek.Item1,
                    SlotId = Guid.Parse(slot.Item1),
                });

                schedules.Add(new(dayOfWeek.Item1, slot.Item2, dayOfWeek.Item2));

            }

            var filterLecturerRequestData = new FilterLecturerRequest
            {
                StartDate = startDate,
                Schedules = scheduleRequests,
                CourseId = course.CourseId.ToString(),
            };

            var filterLecturerContent = new StringContent(JsonSerializer.Serialize(filterLecturerRequestData), Encoding.UTF8, "application/json");
            var lecturereResponses = await _httpClient.PostAsync(baseUrl + "/users/getLecturer", filterLecturerContent);
            var lecturer = new LecturerResponse();
            if (lecturereResponses.IsSuccessStatusCode)
            {
                string content = await lecturereResponses.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LecturerResponse>>(content);
                lecturer = data![random.Next(0, data.Count)];
            }
            return lecturer;
        }

        private static void InitData(out List<(string, string)> roomOnline, out List<(string, string)> roomOffline, out List<(string, string)> slots, out List<(string, int)> dayOfWeeks)
        {
            roomOnline = new List<(string, string)>
            {
                ("GoogleMeet1", "21c2d354-1de5-4b67-950d-326ac832b4eb"),
                ("GoogleMeet2", "472d7b7a-22ce-4fcb-a436-3ea03fd29d78"),
                ("GoogleMeet3", "e1482984-8995-4fcc-8d5b-6014762814e8"),
            };
            roomOffline = new List<(string, string)>
            {
                ("LB1", "40514ccc-b66b-4093-af5f-0445210f9deb"),
                ("105", "db0c0da6-1a17-45a1-aeb6-091db19241fa"),
                ("104", "e6fc0f12-b135-4df4-bfe8-0ade3dcd2f1a"),
                ("210", "c2d5f218-2793-444c-a2d1-18fc81484c4f"),
                ("207", "c0434c38-eb33-4eae-98ff-2d5afc491330"),
                ("102", "f388a94d-d808-40bc-8fa8-ecebf64de0a4"),
                ("103", "cfddfd4d-f80b-4236-921c-599d812f1150"),
                ("101", "99f6f043-3fee-435f-a8ae-1f55f13b3256"),
                ("204", "be0c6afa-e24f-4132-aaa7-bb6408cab5a9"),
                ("LB2", "ea5b5f78-0223-4d7c-8950-4c7beb389e94")
            };
            slots = new List<(string, string)>
            {
            ("417997AC-AFD7-4363-BFE5-6CDD56D4713A", "7:00 - 9:00"),
            ("301EFD4A-618E-4495-8E7E-DAA223D3945E", "9:15 - 11:15"),
            ("6AB50A00-08BA-483C-BF5D-0D55B05A2CCC", "12:00 - 14:00"),
            ("2291E53B-094B-493E-8132-C6494D2B18A8", "14:15 - 16:30"),
            ("688FE18C-5DB1-40AA-A7F3-F47CCD9FD395", "16:30 - 18:30"),
            ("418704FB-FAC8-4119-8795-C8FE5D348753", "19:00 - 21:00")
            };
            dayOfWeeks = new List<(string, int)> { ("monday", 1), ("tuesday", 2), ("wednesday", 3), ("thursday", 4), ("friday", 5), ("saturday", 6), ("sunday", 7) };
        }

        private async Task<List<CourseResExtraInfor>?> FetchCourses()
        {
            var courses = new List<CourseResExtraInfor>();

            var courseApiResponse = await _httpClient.GetAsync(baseUrl + "/courses");

            if (courseApiResponse.IsSuccessStatusCode)
            {
                string content = await courseApiResponse.Content.ReadAsStringAsync();
                courses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CourseResExtraInfor>>(content);
            }

            return courses;
        }
    }
}
