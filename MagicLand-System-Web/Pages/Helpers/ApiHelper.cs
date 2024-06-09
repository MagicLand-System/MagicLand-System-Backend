using MagicLand_System_Web_Dev.Pages.Enums;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MagicLand_System_Web_Dev.Pages.Helper
{
    public class ApiHelper
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string Scheme = "https";
        //private readonly string Scheme = "http";
        private readonly string Domain = "3248-118-69-69-189.ngrok-free.app";
        //private readonly string Domain = "magiclandapiv2.somee.com";
        //private readonly string Domain = "localhost:5097";
        private string RootUrl = "", CallUrl = "", JsonContent = "";

        public ApiHelper(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);
            _httpContextAccessor = httpContextAccessor; ;
        }

        public async Task<ResultHelper<T>> FetchApiAsync<T>(string postFixUrl, MethodEnum method, object data)
        {

            RootUrl = Scheme + "://" + Domain;
            CallUrl = RootUrl + postFixUrl;

            JsonSerializerOptions options = SetHeader();
            JsonContent = data != null ? JsonSerializer.Serialize(data) : "";

            var response = await DoApi(method);
            response.Headers.Add("ngrok-skip-browser-warning", "true");

            int statusCode = (int)response.StatusCode;
            var responseContent = await response.Content.ReadAsStringAsync();

            if (responseContent == null)
            {
                return ResultHelper<T>.DefaultResponse();
            }

            if (statusCode != 200)
            {
                responseContent = Regex.Unescape(responseContent);

            }

            return ResultHelper<T>.Response(response.IsSuccessStatusCode ? JsonSerializer.Deserialize<T>(responseContent!, options)! : default,
                   response!.IsSuccessStatusCode ? "Thành Công" : responseContent!, statusCode.ToString(), statusCode == 200 ? true : false);

        }

        private async Task<HttpResponseMessage> DoApi(MethodEnum method)
        {
            switch (method)
            {
                case MethodEnum.GET:
                    return await _httpClient.GetAsync(CallUrl); ;
                case MethodEnum.POST:
                    return await _httpClient.PostAsync(CallUrl, new StringContent(JsonContent, Encoding.UTF8, "application/json"));
                case MethodEnum.PUT:
                    if (JsonContent != "" || JsonContent != null)
                    {
                        return await _httpClient.PutAsync(CallUrl, new StringContent(JsonContent, Encoding.UTF8, "application/json"));
                    }
                    return await _httpClient.PutAsync(CallUrl, null);
                case MethodEnum.DELETE:
                    return await _httpClient.DeleteAsync(CallUrl);
            }

            return default!;
        }

        private JsonSerializerOptions SetHeader()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,

            };
            var token = SessionHelper.GetObjectFromJson<string>(_httpContextAccessor.HttpContext!.Session, "Token");

            if (token != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return options;
        }

    }
}
