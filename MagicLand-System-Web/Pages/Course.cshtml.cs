using MagicLand_System_Web.Pages.Helper;
using MagicLand_System_Web.Pages.Message;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MagicLand_System_Web.Pages
{
    public class CourseModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public CourseModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public bool IsLoading { get; set; }

        [BindProperty]
        public List<CourseMessage> CourseMessages { get; set; } = new List<CourseMessage>();

        public void OnGet()
        {
            IsLoading = false;
            var data = SessionHelper.GetObjectFromJson<List<CourseMessage>>(HttpContext!.Session, "DataCourse");

            if (data != null && data.Count > 0)
            {
                CourseMessages = data;
            }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            //for (int i = 0; i < 20; i++)
            //{
            //    CourseMessages.Add(new CourseMessage
            //    {
            //        CourseName = "a" + i,
            //        CoursePrice = 20000 + i,
            //        AgeRange = "4-10" + i,
            //        Status = "200" + i,
            //        Note = "note" + i,
            //        SyllabusBelong = "sy" + i,
            //    });
            //}
            //SessionHelper.SetObjectAsJson(HttpContext.Session, "DataCourse", CourseMessages);
            //IsLoading = true;
            //Thread.Sleep(50000);

            //return Page();


            //var syllabusResponses = await _httpClient.GetAsync(baseUrl + "/Syllabus/general");
            //var syllabuses = new List<SyllabusResponseV2>();
            //if (syllabusResponses.IsSuccessStatusCode)
            //{
            //    string content = await syllabusResponses.Content.ReadAsStringAsync();
            //    syllabuses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SyllabusResponseV2>>(content);
            //}

            //if (syllabuses == null || syllabuses.Count == 0)
            //{
            //    ViewData["Message"] = "Các Giáo Trình Đều Đã Có Khóa Học";
            //    return Page();
            //}
            //Random random = new Random();

            //string token = SessionHelper.GetObjectFromJson<string>(HttpContext!.Session, "Token");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //for (int order = 0; order < syllabuses!.Count; order++)
            //{
            //    var subDescription = new List<SubDescriptionRequest>();
            //    var numberSubDescription = random.Next(3, 6);

            //    for (int i = 0; i < numberSubDescription; i++)
            //    {
            //        var numberSubDesctiptionContent = random.Next(2, 4);
            //        var subDescriptionContent = new List<SubDescriptionContentRequest>();

            //        for (int j = 0; j < numberSubDesctiptionContent; j++)
            //        {
            //            subDescriptionContent.Add(new SubDescriptionContentRequest
            //            {
            //                Content = "This is content for sub description of course " + (order + 1) + " no " + (j + 1),
            //                Description = "This is description for content of a course " + (order + 1) + " no " + (j + 1),
            //            });
            //        }

            //        subDescription.Add(new SubDescriptionRequest
            //        {
            //            Title = "This is title for sub description of course " + (order + 1) + " no " + (i + 1),
            //            SubDescriptionContentRequests = subDescriptionContent,
            //        });
            //    }

            //    var courseRequest = new CreateCourseRequest
            //    {
            //        CourseName = syllabuses[order].SyllabusName + "_" + (order + 1),
            //        Price = random.Next(200000, 700000),
            //        MinAge = random.Next(4, 7),
            //        MaxAge = random.Next(7, 11),
            //        MainDescription = "This is main description for course " + (order + 1),
            //        Img = "https://firebasestorage.googleapis.com/v0/b/magic-2e5fc.appspot.com/o/courses%2FTo%C3%A1n%2Ftoan.jpg0c7a07be-eb5a-4da3-8af0-378b29f8a347?alt=media&token=33ae453d-eb1f-4024-9740-9d156b9138e2",
            //        SyllabusId = syllabuses[order].Id.ToString(),
            //        SubDescriptions = subDescription,
            //    };

            //    var courseContent = new StringContent(JsonSerializer.Serialize(courseRequest), Encoding.UTF8, "application/json");
            //    var insertResponse = await _httpClient.PostAsync(baseUrl + "/courses/add", courseContent);

            //    int statusCode = (int)insertResponse.StatusCode;
            //    string responseMessage = await insertResponse.Content.ReadAsStringAsync();

            //    CourseMessages.Add(new CourseMessage
            //    {
            //        CourseName = courseRequest.CourseName,
            //        CoursePrice = courseRequest.Price,
            //        SyllabusBelong = syllabuses[order].SyllabusName,
            //        AgeRange = courseRequest.MinAge + " - " + courseRequest.MaxAge,
            //        Status = statusCode.ToString(),
            //        Note = responseMessage,
            //    });
            //}

            //SessionHelper.SetObjectAsJson(HttpContext.Session, "DataCourse", CourseMessages);
            //IsLoading = true;

            return Page();
        }
    }

}
