using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Attendance
{
    public class StudentAttendanceRequest
    {
        public required Guid StudentId { get; set; }
        public required bool IsAttendance { get; set; } = false;
    }
}
