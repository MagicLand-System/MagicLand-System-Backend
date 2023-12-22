using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class Class
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? ClassCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Street { get; set; } = "Home";
        public string? City { get; set; } = "";
        public string? District { get; set; } = "";
        public string? Status { get; set; }
        public string? Method { get; set; }
        public int LimitNumberStudent { get; set; }
        public int LeastNumberStudent { get; set; }
        public string? Image { get; set; } = null;
        public string? Video { get; set; } = null;

        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        [ForeignKey("User")]
        public Guid LecturerId { get; set; }
        public User? Lecture { get; set; }

        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
