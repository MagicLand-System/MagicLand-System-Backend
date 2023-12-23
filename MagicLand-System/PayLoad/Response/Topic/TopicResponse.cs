using MagicLand_System.PayLoad.Response.Session;

namespace MagicLand_System.PayLoad.Response.Topic
{
    public class TopicResponse
    {
        public string Name { get; set; } = string.Empty;
        public int OrderNumber { get; set; } = 1;

        public List<SessionResponse> Sessions { get; set; } = new List<SessionResponse>();
    }
}
