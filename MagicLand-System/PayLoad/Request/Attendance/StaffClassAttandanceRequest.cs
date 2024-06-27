namespace MagicLand_System.PayLoad.Request.Attendance
{
    public class StaffClassAttandanceRequest
    {
        public Guid Id { get; set; }
        public bool IsPresent { get; set; } = false; 
    }
}
