using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Slots;

namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class ScheduleWithAttendanceResponse : ScheduleResponse
    {
        public List<AttendanceResponse> AttendanceInformation { get; set; } = new List<AttendanceResponse>();
    }
}
