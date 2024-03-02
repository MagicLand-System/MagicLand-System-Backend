namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class SessionWithDateResponse : SessionResponse
    {
        public DateTime? Date { get; set; } = default;
        public string? DateOfWeek { get; set; } = string.Empty;
        public TimeOnly? StartTime { get; set; } = default;
        public TimeOnly? EndTime { get; set;} = default;
    }
}
