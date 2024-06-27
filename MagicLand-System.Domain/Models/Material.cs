using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class Material
    {
        public Guid Id { get; set; }
        public string? URL { get; set; }
        public string? Name { get; set; }
        [ForeignKey("Syllabus")]
        public Guid SyllabusId { get; set; }
        public Syllabus? Syllabus { get; set; }
    }
}
