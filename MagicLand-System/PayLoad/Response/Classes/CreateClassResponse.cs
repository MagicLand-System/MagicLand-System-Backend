using MagicLand_System.PayLoad.Request;
using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class CreateClassResponse
    {
        public Guid? CourseId { get; set; }
        public Guid? LecturerId { get; set; }
        public Guid? RoomId { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Method { get; set; }
        public int? LimitNumberStudent { get; set; }
        public string? ClassCode { get; set; }
        public int? LeastNumberStudent { get; set; }
        public List<ScheduleRequest> ScheduleRequests { get; set; } = new List<ScheduleRequest>();
    }
}
