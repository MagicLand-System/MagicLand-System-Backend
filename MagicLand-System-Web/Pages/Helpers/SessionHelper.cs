using System.Text.Json.Serialization;
using System.Text.Json;

namespace MagicLand_System_Web_Dev.Pages.Helper
{
    public static class SessionHelper
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            return value == null ? default : JsonSerializer.Deserialize<T>(value, options);
        }
    }
}
