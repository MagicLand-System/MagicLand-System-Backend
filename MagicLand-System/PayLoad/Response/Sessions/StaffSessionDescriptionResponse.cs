namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class StaffSessionDescriptionResponse
    {
        public Guid ScheduleId { get; set; }
        public string? Content { get; set; } = string.Empty;
        public List<string>? Details { get; set; } = new List<string>();
    }
}
