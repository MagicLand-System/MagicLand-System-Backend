using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class Rate
    {
        public Guid Id { get; set; }
        public Guid Rater { get; set; }
        public double RateScore { get; set; } = 0;


        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }
    }
}
