using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class CoursePrice
    {
        public Guid Id { get; set; }
        public double Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }   


        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }
    }
}
