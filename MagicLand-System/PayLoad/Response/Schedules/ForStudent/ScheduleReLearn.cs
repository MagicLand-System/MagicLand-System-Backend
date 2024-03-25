namespace MagicLand_System.PayLoad.Response.Schedules.ForStudent
{
    public class ScheduleReLearn
    {
        public DateOnly DayOffRequest { get; set; }
        public List<ScheduleResponse>? Schedules { get; set; }
    }
}
