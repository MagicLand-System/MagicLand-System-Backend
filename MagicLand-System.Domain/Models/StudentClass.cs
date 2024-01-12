using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class StudentClass
    {
        public Guid Id { get; set; }
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public Student? Student { get; set; }

        [ForeignKey("Class")]
        public Guid ClassId { get; set; }
        public Class? Class { get; set; }
    }
}
