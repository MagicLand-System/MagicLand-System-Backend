namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class LectureScheduleResponse : ScheduleResponse
    {
        public required Guid ClassId { get; set; }
        public required string ClassCode { get; set; }
        public required string Method { get; set; }
        //public Guid SessionId { get; set; } = default;
        //public int NumberSession { get; set; } = 0;
    }
}
