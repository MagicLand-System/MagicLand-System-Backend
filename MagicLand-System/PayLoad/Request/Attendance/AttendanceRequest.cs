namespace MagicLand_System.PayLoad.Request.Attendance
{
    public class AttendanceRequest
    {
        public required Guid ClassId { get; set; }
        public required List<StudentAttendanceRequest> StudentAttendanceRequests { get; set; }
    }
}
