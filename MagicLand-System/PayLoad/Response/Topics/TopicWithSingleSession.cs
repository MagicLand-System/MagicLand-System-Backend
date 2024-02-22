using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.PayLoad.Response.Topics
{
    public class TopicWithSingleSession
    {
        public string? TopicName { get; set; }
        public int? OrderNumber { get; set; }

        public SessionResponse? Session { get; set; }
    }
}
