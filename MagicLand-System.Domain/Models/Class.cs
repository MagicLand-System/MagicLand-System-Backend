using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Class
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? ClassCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Address { get; set; } = "Home";
        public string? Status { get; set; }
        public string? Method { get; set; }
        public double Price { get; set; }
        public int LimitNumberStudent { get; set; }
        public int LeastNumberStudent { get; set; }
        public string? Image { get; set; } = null;
        public string? Video { get; set; } = null;

        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public required Course Course { get; set; }

        [ForeignKey("User")]
        public Guid LecturerId { get; set; }
        public User? Lecture { get; set; }
    
        
        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
