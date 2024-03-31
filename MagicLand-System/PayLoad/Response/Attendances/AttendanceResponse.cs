namespace MagicLand_System.PayLoad.Response.Attendances
{
    public class AttendanceResponse
    {
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public bool? IsPresent { get; set; }
        public string? Note { get; set; }
    }
}
