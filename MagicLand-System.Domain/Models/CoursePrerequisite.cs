using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class CoursePrerequisite
    {
        public Guid Id { get; set; }
        public Guid PrerequisiteCourseId { get; set; }

        [ForeignKey("Course")]
        public Guid CurrentCourseId { get; set; }
        public Course? Course { get; set; }

    }
}
