using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class CourseSyllabus
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime? EffectiveDate { get; set; } = default;
        public string? StudentTasks { get; set; }
        public double ScoringScale {  get; set; } 
        public Guid? CourseId { get; set; } = null;
        public Course? Course { get; set; }
        public int TimePerSession { get; set; }  
        public double MinAvgMarkToPass { get; set; }
        public string? Description { get; set; } 
        public string? SyllabusLink { get; set; }
        public string? SubjectCode { get; set; }
        public ICollection<Topic> Topics { get; set; } = new List<Topic>();
        public ICollection<Material> Materials { get; set; } = new List<Material>();
        public ICollection<ExamSyllabus> ExamSyllabuses { get; set; } = new List<ExamSyllabus>(); 
    }
}
