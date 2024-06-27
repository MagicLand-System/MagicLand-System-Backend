namespace MagicLand_System.PayLoad.Request
{
    public class ScheduleRequest
    {
        public string DateOfWeek {  get; set; }
        public Guid SlotId {  get; set; }
        public string StartTime {  get; set; }  
        public string EndTime { get; set; }

    }
}
