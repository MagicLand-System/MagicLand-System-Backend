using System.ComponentModel.DataAnnotations.Schema;

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
