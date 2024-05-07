using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System_Web.Pages.Enums;
using MagicLand_System_Web.Pages.Helper;
using MagicLand_System_Web.Pages.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace MagicLand_System_Web.Pages
{
    public class StudentModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public StudentModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public List<StudentMessage> StudentMessages { get; set; } = new List<StudentMessage>();


        [BindProperty]
        public bool IsLoading { get; set; }

        [BindProperty]
        public List<User> Parents { get; set; } = new List<User>();
        public async Task<IActionResult> OnGetAsync()
        {
            IsLoading = false;
            var data = SessionHelper.GetObjectFromJson<List<StudentMessage>>(HttpContext.Session, "DataStudent");
            var parents = SessionHelper.GetObjectFromJson<List<User>>(HttpContext.Session, "Parents");
            if (data != null && data.Count > 0)
            {
                StudentMessages = data;
            }


            if (parents != null && parents.Count > 0)
            {
                Parents = parents;
            }
            else
            {
                var result = await _apiHelper.FetchApiAsync<List<User>>(ApiEndpointConstant.UserEndpoint.RootEndpoint, MethodEnum.GET, null);

                if (result.IsSuccess)
                {
                    if (result.Data == null)
                    {
                        SessionHelper.SetObjectAsJson(HttpContext.Session, "Parents", Parents);
                    }
                    else
                    {
                        Parents = result.Data.Where(x => x.Role!.Name == RoleEnum.PARENT.ToString()).ToList();
                        SessionHelper.SetObjectAsJson(HttpContext.Session, "Parents", result.Data!);
                    }

                    return Page();
                }

            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int inputField, string listId, string submitButton)
        {
            if (submitButton == "Refresh")
            {
                StudentMessages.Clear();

                var result = await _apiHelper.FetchApiAsync<List<User>>(ApiEndpointConstant.UserEndpoint.RootEndpoint, MethodEnum.GET, null);

                if (result.IsSuccess)
                {
                    Parents = result.Data.Where(x => x.Role!.Name == RoleEnum.PARENT.ToString()).ToList();
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "Parents", result.Data!);
                    IsLoading = true;
                    return Page();
                }
            }

            if (inputField == 0 || inputField < 0 || inputField >= 100)
            {
                ViewData["Message"] = "Số Lượng không Hợp Lệ";
                var result = await _apiHelper.FetchApiAsync<List<User>>(ApiEndpointConstant.UserEndpoint.RootEndpoint, MethodEnum.GET, null);

                if (result.IsSuccess)
                {
                    Parents = result.Data.Where(x => x.Role!.Name == RoleEnum.PARENT.ToString()).ToList();
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "Parents", result.Data!);
                    IsLoading = true;
                    return Page();
                }
                return Page();
            }
            ViewData["Message"] = "";

            var idParses = new List<Guid>();
            if (!string.IsNullOrEmpty(listId))
            {
                string pattern = @"\|([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\|";
                MatchCollection matches = Regex.Matches(listId, pattern);

                foreach (Match match in matches)
                {
                    idParses.Add(Guid.Parse(match.Groups[1].Value));
                }
            }

            var parents = SessionHelper.GetObjectFromJson<List<User>>(HttpContext!.Session, "Parents");
            if (idParses.Any())
            {
                parents = idParses.Select(id => parents.Single(c => c.Id == id)).ToList();
            }
            Random random = new Random();

            foreach (var parent in parents)
            {
                for (int order = 0; order < inputField; order++)
                {

                }
            }

            SessionHelper.SetObjectAsJson(HttpContext.Session, "DataStudent", "");
            IsLoading = true;

            return Page();
        }
    }
}
