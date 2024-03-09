namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class SessionSyllabusResponse
    {
        public int OrderSession { get; set; } = 1;
        public string? Date { get; set; } = string.Empty;
        public string? DateOfWeek { get; set; } = string.Empty;
        public TimeOnly? StartTime { get; set; } = default;
        public TimeOnly? EndTime { get; set; } = default;
        public List<SessionContentReponse>? Contents { get; set; } = new List<SessionContentReponse>();
    }
}
