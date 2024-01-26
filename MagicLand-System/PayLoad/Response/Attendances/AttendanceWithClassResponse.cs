using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Attendances
{
    public class AttendanceWithClassResponse
    {
        public Guid? ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? ClassCode { get; set; }
        public UserResponse? Lecture { get; set; } 
        public List<ScheduleWithAttendanceResponse> Schedules { get; set; } = new List<ScheduleWithAttendanceResponse>();
    }
}
