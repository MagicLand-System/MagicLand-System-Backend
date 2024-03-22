using Academic_Blog_App.Services.Helper;
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
            baseUrl = "https://magiclandapiv2.somee.com/api/v1";
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public void OnGet()
        {

        }
        [BindProperty]
        public List<(string, string, string)> messages { get; set; } = default!;
        public async Task<IActionResult> OnPostAsync(int inputField)
        {

            if (inputField == 0 || inputField < 0 || inputField >= 100)
            {
                ViewData["Message"] = "Số Lượng không Hợp Lệ";
                return Page();
            }
           var courses = await FetchCourses();

            Random random = new Random();

            for (int i = 0; i < inputField; i++)
            {
                int index = random.Next(0, courses!.Count());

                await RenderProgress(courses, i, index);
            }

            return Page();
        }

        private async Task RenderProgress(List<CourseResExtraInfor>? courses, int i, int index)
        {
          

            var lectureres = await _httpClient.GetAsync(baseUrl + "/getLecturer");

            var schedules = new List<ScheduleRequest>
                {
                    new ScheduleRequest
                    {
                        DateOfWeek = "monday",
                        SlotId = Guid.Parse("417997ac-afd7-4363-bfe5-6cdd56d4713a"),
                    }
                };

            var requestData = new CreateClassRequest
            {
                ClassCode = courses![index].CourseDetail!.SubjectCode + "-" + i,
                CourseId = courses![index].CourseId,
                StartDate = DateTime.Now.AddDays(1),
                LeastNumberStudent = 5,
                LimitNumberStudent = 30,
                LecturerId = Guid.Parse("2a95e7fd-010a-4f73-870b-1a0610c7fefa"),
                Method = index % 2 == 0 ? "OFLINE" : "ONLINE",
                ScheduleRequests = schedules,
                RoomId = index % 2 == 0 ? Guid.Parse("21c2d354-1de5-4b67-950d-326ac832b4eb") : Guid.Parse("40514ccc-b66b-4093-af5f-0445210f9deb"),
            };


            string token = SessionHelper.GetObjectFromJson<string>(_httpContextAccessor.HttpContext!.Session, "Token");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
            var insertResponse = await _httpClient.PostAsync(baseUrl + "/classes/add", jsonContent);

            int statusCode = (int)insertResponse.StatusCode;

            messages.Add(new("Lớp Thứ Tự " + i, $"{courses![index].CourseDetail!.CourseName}", $"{statusCode}"));
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
