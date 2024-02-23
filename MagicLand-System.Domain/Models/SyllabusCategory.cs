namespace MagicLand_System.Domain.Models
{
    public class SyllabusCategory
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } 

        public ICollection<Syllabus> Syllabuses { get; set; } = new List<Syllabus>();
    }
}
