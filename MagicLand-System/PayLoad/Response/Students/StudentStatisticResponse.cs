namespace MagicLand_System.PayLoad.Response.Students
{
    public class StudentStatisticResponse : StudentResponse
    {
        public required string ParentName { get; set; }
        public required DateTime AddedTime { get; set; }
    }
}
