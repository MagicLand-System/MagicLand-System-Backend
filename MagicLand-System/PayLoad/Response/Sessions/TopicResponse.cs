namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class TopicResponse
    {
        public Guid TopicId { get; set; }
        public string? TopicName { get; set; } = string.Empty;
        public int OrderTopic { get; set; } = 1;
        public List<StaffSessionResponse>? Sessions { get; set; } = new List<StaffSessionResponse>();
    }
}
