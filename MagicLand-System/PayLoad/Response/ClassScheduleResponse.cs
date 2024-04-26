namespace MagicLand_System.PayLoad.Response
{
    public class ClassScheduleResponse
    {
        public Guid Id { get; set; }
        public int Index { get; set; }  
        public string Status { get; set; }  
        public DateTime Date { get; set; }
        public TopicContent TopicContent { get; set; } 
        public string StartTime { get; set; }   
        public string EndTime { get; set; }
    }
}
