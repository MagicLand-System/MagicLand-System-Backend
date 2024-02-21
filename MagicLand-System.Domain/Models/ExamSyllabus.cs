using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class ExamSyllabus
    {
        public Guid Id { get; set; }
        public string Category {  get; set; }   
        public double Weight { get; set; }
        public double CompleteionCriteria {  get; set; }    
        public string Duration {  get; set; }   
        public string QuestionType {  get; set; }   
        public Guid CourseSyllabusId { get; set; }
        public CourseSyllabus CourseSyllabus { get; set; }
    }
}
