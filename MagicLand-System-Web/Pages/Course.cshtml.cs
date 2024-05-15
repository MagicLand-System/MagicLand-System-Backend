using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Syllabuses.ForStaff;
using MagicLand_System_Web_Dev.Pages.DataContants;
using MagicLand_System_Web_Dev.Pages.Enums;
using MagicLand_System_Web_Dev.Pages.Helper;
using MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MagicLand_System_Web_Dev.Pages
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
        public List<CourseDefaultMessage> CourseMessages { get; set; } = new List<CourseDefaultMessage>();
        [BindProperty]
        public List<SyllabusResponseV2> ValidSyllabus { get; set; } = new List<SyllabusResponseV2>();
        public async Task<IActionResult> OnGet()
        {
            IsLoading = false;
            var data = SessionHelper.GetObjectFromJson<List<CourseDefaultMessage>>(HttpContext!.Session, "DataCourse");
            var validSyllabus = SessionHelper.GetObjectFromJson<List<SyllabusResponseV2>>(HttpContext!.Session, "ValidSyllabus");


            if (data != null && data.Count > 0)
            {
                CourseMessages = data;
            }

            if (validSyllabus != null && validSyllabus.Count > 0)
            {
                ValidSyllabus = validSyllabus;
            }
            else
            {
                var result = await _apiHelper.FetchApiAsync<List<SyllabusResponseV2>>(ApiEndpointConstant.SyllabusEndpoint.AvailableSyl, MethodEnum.GET, null);

                if (result.IsSuccess)
                {
                    if (result.Data == null)
                    {
                        SessionHelper.SetObjectAsJson(HttpContext.Session, "ValidSyllabus", ValidSyllabus);
                    }
                    else
                    {
                        ValidSyllabus = result.Data;
                        SessionHelper.SetObjectAsJson(HttpContext.Session, "ValidSyllabus", result.Data!);
                    }

                    return Page();
                }

            }

            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string submitButton)
        {
            if (submitButton == "Refresh")
            {
                CourseMessages.Clear();

                var result = await _apiHelper.FetchApiAsync<List<SyllabusResponseV2>>(ApiEndpointConstant.SyllabusEndpoint.AvailableSyl, MethodEnum.GET, null);

                if (result.IsSuccess)
                {
                    ValidSyllabus = result.Data;
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "ValidSyllabus", result.Data);
                    IsLoading = true;
                    return Page();
                }
            }

            var validSyllabus = SessionHelper.GetObjectFromJson<List<SyllabusResponseV2>>(HttpContext!.Session, "ValidSyllabus");
            if (validSyllabus == null || validSyllabus.Count == 0)
            {
                return Page();
            }

            Random random = new Random();
            var storedIndex = new List<int>();
            var numberSubDescription = random.Next(3, 6);
            var numberSubDesctiptionContent = random.Next(2, 4);

            for (int order = 0; order < validSyllabus.Count; order++)
            {
                var subDescription = new List<SubDescriptionRequest>();

                for (int i = 0; i < numberSubDescription; i++)
                {
                    var title = CourseData.TitleSubDescriptions[random.Next(0, CourseData.TitleSubDescriptions.Count)];
                    var subDescriptionContent = new List<SubDescriptionContentRequest>();

                    storedIndex.Clear();

                    for (int j = 0; j < numberSubDesctiptionContent; j++)
                    {
                        var subContent = CourseData.GetSubDescription(title.Item2, random, storedIndex);
                        storedIndex.Add(subContent.Item2);

                        subDescriptionContent.Add(new SubDescriptionContentRequest
                        {
                            Content = subContent.Item1.Item1,
                            Description = subContent.Item1.Item2,
                        });
                    }

                    subDescription.Add(new SubDescriptionRequest
                    {
                        Title = title.Item1,
                        SubDescriptionContentRequests = subDescriptionContent,
                    });
                }

                var priceValue = random.Next(20, 71) + "0000";
                var objectRequest = new CreateCourseRequest
                {
                    CourseName = validSyllabus[order].SyllabusName + "_" + (order + 1),
                    Price = int.Parse(priceValue),
                    MinAge = random.Next(4, 7),
                    MaxAge = random.Next(7, 11),
                    MainDescription = CourseData.MainDescriptions[random.Next(0, CourseData.MainDescriptions.Count)],
                    Img = "https://firebasestorage.googleapis.com/v0/b/magic-2e5fc.appspot.com/o/courses%2FTo%C3%A1n%2Ftoan.jpg0c7a07be-eb5a-4da3-8af0-378b29f8a347?alt=media&token=33ae453d-eb1f-4024-9740-9d156b9138e2",
                    SyllabusId = validSyllabus[order].Id.ToString(),
                    SubDescriptions = subDescription,
                };

                var result = await _apiHelper.FetchApiAsync<bool>(ApiEndpointConstant.CourseEndpoint.AddCourse, MethodEnum.POST, objectRequest);

                CourseMessages.Add(new CourseDefaultMessage
                {
                    CourseName = objectRequest.CourseName,
                    CoursePrice = objectRequest.Price,
                    SyllabusBelong = validSyllabus[order].SyllabusName,
                    AgeRange = objectRequest.MinAge + " - " + objectRequest.MaxAge,
                    Status = result.StatusCode,
                    Note = result.Message,
                });
            }

            SessionHelper.SetObjectAsJson(HttpContext.Session, "DataCourse", CourseMessages);
            IsLoading = true;
            HttpContext.Session.Remove("ValidSyllabus");
            return Page();
        }
    }

}
