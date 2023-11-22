using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Response.Address;
using MagicLand_System.PayLoad.Response.Session;
using MagicLand_System.PayLoad.Response.User;

namespace MagicLand_System.PayLoad.Response.Class
{
    public class ClassResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid CourseId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Method { get; set; } = default!;
        public decimal Price { get; set; }
        public int LimitNumberStudent { get; set; }
        public int NumberStudentRegistered { get; set; }
        public string? Status { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
        public int MinYearOldsStudent { get; set; }
        public int MaxYearOldsStudent { get; set; }
        public AddressResponse? Address { get; set; }
        public UserResponse? Lecture { get; set; }
        public List<SessionResponse>? Sessions { get; set; }

    }
}
