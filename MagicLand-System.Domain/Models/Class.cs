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
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
        [ForeignKey("User")]
        public Guid LecturerId { get; set; }
        public User User { get; set; }
        [ForeignKey("Address")]
        public Guid? AddressId { get; set; } = null;
        public Address? Address { get; set; }
        public int LimitNumberStudent { get; set; }
        [ForeignKey("Course")]
        public Guid CourseId { get; set; }  
        public Course Course { get; set; }
        public double Price { get; set; }
    }
}
