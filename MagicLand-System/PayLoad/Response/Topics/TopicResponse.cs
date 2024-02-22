using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.PayLoad.Response.Topics
{
    public class TopicResponse
    {
        public string? TopicName { get; set; }
        public int? OrderNumber { get; set; }

        public List<SyllabusInforResponse>? Sessions { get; set; } = new List<SyllabusInforResponse>();
    }
}
