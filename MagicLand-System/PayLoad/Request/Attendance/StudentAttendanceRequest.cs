using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Attendance
{
    public class StudentAttendanceRequest
    {
        public required Guid StudentId { get; set; }
        public required bool IsPresent { get; set; } = false;
        public string? Note { get; set; } = string.Empty;
    }
}
