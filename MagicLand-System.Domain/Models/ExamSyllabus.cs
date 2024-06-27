using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class ExamSyllabus
    {
        public Guid Id { get; set; }
        public string? Category { get; set; }   // it is type in FE need.
        public double Weight { get; set; }
        public double CompletionCriteria { get; set; }
        public string? QuestionType { get; set; }
        public int Part { get; set; }
        public string? ContentName { get; set; }
        public string? Method { get; set; }
        public string? Duration { get; set; }

        [ForeignKey("Syllabus")]
        public Guid SyllabusId { get; set; }
        public Syllabus? Syllabus { get; set; }
    }
}
