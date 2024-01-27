using MagicLand_System.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MagicLand_System.PayLoad.Request.Attendance
{
    public class AttendanceRequest
    {
        public required Guid ClassId { get; set; }
        [Range(1, 6, ErrorMessage = "Slot Học Phải Bắt Đầu Từ 1 Đến 6")]
        public required SlotEnum Slot { get; set; }
        public required List<StudentAttendanceRequest> StudentAttendanceRequests { get; set; }
    }

}
