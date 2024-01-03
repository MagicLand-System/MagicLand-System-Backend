using MagicLand_System.PayLoad.Response.Schedule;
using MagicLand_System.PayLoad.Response.User;

namespace MagicLand_System.PayLoad.Response.Class
{
    public class MyClassResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid CourseId { get; set; }
        public double? CoursePrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public string? Method { get; set; }
        public int LimitNumberStudent { get; set; }
        public int LeastNumberStudent { get; set; }
        public int NumberStudentRegistered { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
        public string? ClassCode { get; set; }

        public string? LecturerName { get; set; }
        public List<DailySchedule>? Schedules { get; set; }
        public string? CourseName {  get; set; } 
    }
}
