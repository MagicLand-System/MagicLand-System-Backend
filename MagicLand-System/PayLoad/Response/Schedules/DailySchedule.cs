namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class DailySchedule
    {
        public string? DayOfWeek { get; set; }
        public string? StartTime {  get; set; }
        public string? EndTime { get; set;}
        public Guid? SlotId {  get; set; }  
        public int? DateOfWeekNumber { get; set; }  
    }
}
