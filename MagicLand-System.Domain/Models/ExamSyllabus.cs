using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class ExamSyllabus
    {
        public Guid Id { get; set; }
        public string? Category { get; set; }   // it is type in FE need.
        public string? Name { get; set; }
        public int ExamOrder { get; set; }
        public double Weight { get; set; }
        public double CompleteionCriteria { get; set; }
        public string? Duration { get; set; }
        public string? QuestionType { get; set; }
        public int Part { get; set; }
        public string? ContentName { get; set; } 
        public string? Method { get; set; }
        [ForeignKey("Syllabus")]
        public Guid SyllabusId { get; set; }
        public Syllabus? Syllabus { get; set; }
    }
}
