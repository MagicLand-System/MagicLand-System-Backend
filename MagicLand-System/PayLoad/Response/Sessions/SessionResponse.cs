using MagicLand_System.PayLoad.Request.Course;

namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class SessionResponse
    {
        public int OrderSession { get; set; } = 1;
        public string? TopicName { get; set; } = string.Empty;
        public int OrderTopic { get; set; } = 1;
        public List<SessionContentReponse>? Contents { get; set; } = new List<SessionContentReponse>();

    }
}
