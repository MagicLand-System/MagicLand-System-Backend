using MagicLand_System.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MagicLand_System.PayLoad.Request.Attendance
{
    public class AttendanceRequest
    {
        public required Guid ClassId { get; set; }
        public required List<StudentAttendanceRequest> StudentAttendanceRequests { get; set; }
    }

}
