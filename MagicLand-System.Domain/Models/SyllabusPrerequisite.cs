using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class SyllabusPrerequisite
    {
        public Guid Id { get; set; }
        public Guid PrerequisiteCourseId { get; set; }

        [ForeignKey("Syllabus")]
        public Guid CurrentSyllabusId { get; set; }
        public Syllabus? Syllabus { get; set; }

    }
}
