namespace MagicLand_System.PayLoad.Request
{
    public class FilterRoomRequest
    {
        public DateTime? StartDate {  get; set; }   
        public List<ScheduleRequest>? Schedules { get; set; }    
        public string? CourseId { get; set; }
        public string? Method { get; set; } 
    }
}
