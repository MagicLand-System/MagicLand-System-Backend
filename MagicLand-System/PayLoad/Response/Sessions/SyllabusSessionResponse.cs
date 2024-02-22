using MagicLand_System.PayLoad.Request.Course;

namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class SyllabusSessionResponse
    {
        public string? TopicName { get; set; } = string.Empty;
        public List<SessionResponse>? Sessions { get; set; } = new List<SessionResponse>();

    }
}
