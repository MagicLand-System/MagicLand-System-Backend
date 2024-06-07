using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System_Web_Dev.Pages.Enums;
using MagicLand_System_Web_Dev.Pages.Helper;
using MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage;
using MagicLand_System_Web_Dev.Pages.Messages.InforMessage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MagicLand_System_Web_Dev.Pages
{
    public class SettingTimeModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public SettingTimeModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public string CurrentTime { get; set; } = string.Empty;
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _apiHelper.FetchApiAsync<DateTime>("/System/GetTime", MethodEnum.GET, null);
                if (result.IsSuccess)
                {
                    CurrentTime = result.Data.ToString("yyyy-MM-ddTHH:mm");
                }
                else
                {
                    return RedirectToPage("/Error");
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostProgressAsync(string time, string submitButton)
        {
            if (submitButton == "Refresh")
            {

                var resetTime = await _apiHelper.FetchApiAsync<string>("/System/ResetTime", MethodEnum.POST, null);
                var getTime = await _apiHelper.FetchApiAsync<DateTime>("/System/GetTime", MethodEnum.GET, null);
                if (getTime.IsSuccess && resetTime.IsSuccess)
                {
                    CurrentTime = getTime.Data.ToString("yyyy-MM-ddTHH:mm");
                    return Page();
                }
                else
                {
                    return RedirectToPage("/Error");
                }
            }

            var setTime = await _apiHelper.FetchApiAsync<string>("/System/SetTime", MethodEnum.POST, DateTime.Parse(time));
            if (setTime.IsSuccess)
            {
                var getTime = await _apiHelper.FetchApiAsync<DateTime>("/System/GetTime", MethodEnum.GET, null);
                CurrentTime = getTime.Data.ToString("yyyy-MM-ddTHH:mm");
                ViewData["Message"] = "Setting Success";
                return Page();
            }
            else
            {
                return RedirectToPage("/Error");
            }
        }
    }
}
