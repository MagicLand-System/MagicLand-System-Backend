using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class SyllabusResponse
    {
        public Guid? SyllabusId { get; set; } = default;
        public string Name { get; set; } = string.Empty;
        public DateTime UpdateTime { get; set; } = default;

        public List<TopicResponse> Topics { get; set; } = new List<TopicResponse>();
    }
}
