using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class ClassForAttendance
    {
        public Guid ClassId { get; set; }
        public string? CourseName { get; set; }
        public string? ClassSubject { get; set; }
        public Guid CourseId { get; set; }
        public double? CoursePrice { get; set; }
        public required double CourseRate { get; set; }
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
        public string? ClassCode { get; set; }
        public Schedule Schedule { get; set; }
        public LecturerResponse Lecturer { get; set; }
        public string AttandanceStatus { get; set; }  
    }
}
