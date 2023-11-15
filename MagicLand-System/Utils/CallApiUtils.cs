using System.Text;

namespace MagicLand_System.Utils
{
    public class CallApiUtils
    {
        public static async Task<HttpResponseMessage> CallApiEndpoint(string url, object data)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(url, content);
            return response;
        }

    }
}
