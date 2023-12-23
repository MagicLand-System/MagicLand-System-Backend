using MagicLand_System.PayLoad.Response.Topic;

namespace MagicLand_System.PayLoad.Response.Syllabus
{
    public class SyllabusResponse
    {
        public string Name { get; set; } = string.Empty;
        public DateTime UpdateTime { get; set; } = default;

        public List<TopicResponse> Topics { get; set; } = new List<TopicResponse>();
    }
}
