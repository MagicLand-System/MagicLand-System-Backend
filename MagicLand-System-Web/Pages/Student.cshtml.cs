using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System_Web_Dev.Pages.DataContants;
using MagicLand_System_Web_Dev.Pages.Enums;
using MagicLand_System_Web_Dev.Pages.Helper;
using MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage;
using MagicLand_System_Web_Dev.Pages.Messages.InforMessage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace MagicLand_System_Web_Dev.Pages
{
    public class StudentModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public StudentModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public List<StudentDefaultMessage> StudentMessages { get; set; } = new List<StudentDefaultMessage>();


        [BindProperty]
        public bool IsLoading { get; set; }

        [BindProperty]
        public List<LoginResponse> Parents { get; set; } = new List<LoginResponse>();
        public async Task<IActionResult> OnGetAsync()
        {
            IsLoading = false;
            var messages = SessionHelper.GetObjectFromJson<List<StudentDefaultMessage>>(HttpContext.Session, "DataStudent");
            var parents = SessionHelper.GetObjectFromJson<List<LoginResponse>>(HttpContext.Session, "Parents");
            if (messages != null && messages.Count > 0)
            {
                StudentMessages = messages;
            }


            if (parents != null && parents.Count > 0)
            {
                Parents = parents;
            }
            else
            {
                await FetchParent();

                return Page();
            }

            return Page();
        }

        private async Task FetchParent()
        {
            var result = await _apiHelper.FetchApiAsync<List<User>>(ApiEndpointConstant.UserEndpoint.RootEndpoint + "?role=" + RoleEnum.PARENT.ToString(), MethodEnum.GET, null);

            if (result.Data == null)
            {
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Parents", Parents);
            }
            else
            {
                foreach (var user in result.Data)
                {
                    var authen = await _apiHelper.FetchApiAsync<LoginResponse>(ApiEndpointConstant.AuthenticationEndpoint.Authentication, MethodEnum.POST, new LoginRequest { Phone = user.Phone! });
                    Parents.Add(authen.Data);
                }

                SessionHelper.SetObjectAsJson(HttpContext.Session, "Parents", Parents);
            }
        }

        public async Task<IActionResult> OnPostProgressAsync(int inputField, string listPhone, string submitButton)
        {
            if (submitButton == "Refresh")
            {
                StudentMessages.Clear();
                Parents = SessionHelper.GetObjectFromJson<List<LoginResponse>>(HttpContext.Session, "Parents");
                return Page();
            }

            if (inputField == 0 || inputField < 0 || inputField >= 100)
            {
                ViewData["Message"] = "Số Lượng không Hợp Lệ";
                Parents = SessionHelper.GetObjectFromJson<List<LoginResponse>>(HttpContext.Session, "Parents");
                IsLoading = true;
                return Page();
            }

            ViewData["Message"] = "";

            var phoneParses = new List<string>();
            if (!string.IsNullOrEmpty(listPhone))
            {
                string pattern = @"\|([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\|";
                MatchCollection matches = Regex.Matches(listPhone, pattern);

                foreach (Match match in matches)
                {
                    phoneParses.Add(match.Groups[1].Value);
                }
            }

            var parents = SessionHelper.GetObjectFromJson<List<LoginResponse>>(HttpContext!.Session, "Parents");
            if (phoneParses.Any())
            {
                parents = phoneParses.Select(phone => parents.Single(c => c.Phone == phone)).ToList();
            }
            Random random = new Random();

            foreach (var parent in parents)
            {
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", parent.AccessToken!);
                for (int order = 0; order < inputField; order++)
                {
                    string[] words = parent.FullName!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    string firstStudentName = words.Length > 0 ? words[0] : "";
                    string studentFullName = firstStudentName + " " + StudentData.StudentMiddleNames[random.Next(0, StudentData.StudentMiddleNames.Count)]
                        + " " + StudentData.StudentLastNames[random.Next(0, StudentData.StudentLastNames.Count)];

                    var gender = StudentData.Genders[random.Next(0, StudentData.Genders.Count)];
                    var studentRequest = new CreateStudentRequest
                    {
                        FullName = studentFullName,
                        DateOfBirth = DateTime.Now.AddYears(random.Next(-10, -4)),
                        Gender = gender,
                        AvatarImage = StudentData.GetStudentImage(gender, random),
                    };

                    var result = await _apiHelper.FetchApiAsync<AccountResponse>(ApiEndpointConstant.StudentEndpoint.StudentEnpointCreate, MethodEnum.POST, studentRequest);
                    if (result.IsSuccess)
                    {
                        StudentMessages.Add(new StudentDefaultMessage
                        {
                            StudentName = studentRequest.FullName,
                            AccountArise = result.Data.AccountPhone,
                            ParentBelong = parent.FullName,
                            Gender = studentRequest.Gender,
                            Age = DateTime.Now.Year - studentRequest.DateOfBirth.Year,
                            Status = result.StatusCode,
                            Note = result.Message,
                        });
                    }
                    else
                    {
                        StudentMessages.Add(new StudentDefaultMessage
                        {
                            StudentName = studentRequest.FullName,
                            AccountArise = "Không",
                            ParentBelong = parent.FullName,
                            Gender = studentRequest.Gender,
                            Age = DateTime.Now.Year - studentRequest.DateOfBirth.Year,
                            Status = result.StatusCode,
                            Note = result.Message,
                        });
                    }
                }
            }

            SessionHelper.SetObjectAsJson(HttpContext.Session, "DataStudent", StudentMessages);
            var defaultToken = SessionHelper.GetObjectFromJson<string>(HttpContext.Session, "DeveloperToken");
            SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", defaultToken);
            IsLoading = true;

            return Page();
        }


        public IActionResult OnPostSearch(string searchKey, string searchType)
        {

            if (string.IsNullOrEmpty(searchKey))
            {
                StudentMessages.Clear();
                Parents = SessionHelper.GetObjectFromJson<List<LoginResponse>>(HttpContext.Session, "Parents");
                return Page();
            }

            var key = searchKey.Trim().ToLower();
            if (searchType == "MESSAGE")
            {
                var messages = SessionHelper.GetObjectFromJson<List<StudentDefaultMessage>>(HttpContext.Session, "DataStudent");

                StudentMessages = messages.Where(
                   mess => mess.StudentName.ToLower().Contains(key) ||
                   mess.ParentBelong.ToLower().Contains(key) ||
                   mess.AccountArise.ToLower().Contains(key)
                   ).ToList();
            }
            if (searchType == "DATA")
            {
                var parents = SessionHelper.GetObjectFromJson<List<LoginResponse>>(HttpContext.Session, "Parents");

                Parents = parents.Where(
                    c => c.FullName!.ToLower().Contains(key) ||
                    c.Phone.ToLower().Contains(key)
                    ).ToList();
            }

            return Page();
        }
    }
}
