using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.PayLoad.Response.Topics
{
    public class TopicResponse
    {
        public int OrderTopic { get; set; } = 1;
        public string? TopicName { get; set; } = string.Empty;
        public List<SessionSyllabusResponse>? Sessions { get; set; } = new List<SessionSyllabusResponse>();
    }
}
