using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Response.Address;
using MagicLand_System.PayLoad.Response.Schedule;
using MagicLand_System.PayLoad.Response.Session;
using MagicLand_System.PayLoad.Response.User;

namespace MagicLand_System.PayLoad.Response.Class
{
    public class ClassResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Address { get; set; } = "Home";
        public string? Status { get; set; }
        public string? Method { get; set; }
        public int LimitNumberStudent { get; set; }
        public int LeastNumberStudent { get; set; }
        public int NumberStudentRegistered { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }

        public UserResponse? Lecture { get; set; }
        public List<ScheduleResponse>? Schedules { get; set; }

    }
}
