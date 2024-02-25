using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Syllabus
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


        [ForeignKey("SyllabusCategory")]
        public Guid SyllabusCategoryId { get; set; }
        public SyllabusCategory? SyllabusCategory { get; set; }

        public ICollection<Topic>? Topics { get; set; }
        public ICollection<Material>? Materials { get; set; }
        public ICollection<ExamSyllabus>? ExamSyllabuses { get; set; }
        public ICollection<SyllabusPrerequisite> SyllabusPrerequisites { get; set; }
    }
}
