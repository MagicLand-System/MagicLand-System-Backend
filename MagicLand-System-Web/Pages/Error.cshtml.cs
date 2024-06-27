using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Response;
using MagicLand_System_Web_Dev.Pages.Enums;
using MagicLand_System_Web_Dev.Pages.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace MagicLand_System_Web_Dev.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string? RequestId { get; set; }
        public string Error { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        private readonly ApiHelper _apiHelper;

        public ErrorModel(ILogger<ErrorModel> logger, IHttpContextAccessor httpContextAccessor, ApiHelper apiHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiHelper = apiHelper;
        }

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            Error = "Session Timeout";
        }

        public async Task<IActionResult> OnPost(string Page)
        {
            _httpContextAccessor.HttpContext.Session.Clear();

            var objectRequest = new LoginRequest
            {
                Phone = "+84971822093",
            };

            var result = await _apiHelper.FetchApiAsync<LoginResponse>(ApiEndpointConstant.AuthenticationEndpoint.Authentication, MethodEnum.POST, objectRequest);

            if (result.IsSuccess)
            {
                var user = result.Data;
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", user!.AccessToken);
                SessionHelper.SetObjectAsJson(HttpContext.Session, "DeveloperToken", user!.AccessToken);
            }
            return RedirectToPage("/" + Page);
        }
    }
}
