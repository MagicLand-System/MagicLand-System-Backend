using MagicLand_System.PayLoad.Request.Course;

namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class SessionContentReponse
    {
        public string? Content { get; set; } = string.Empty;
        public List<string>? Details { get; set; } = new List<string>();
    }
}
