using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int NumberOfSession { get; set; }
        public int MinYearOldsStudent { get; set; } = 3;
        public int MaxYearOldsStudent { get; set; } = 120;
        public string Status { get; set; }
        public ICollection<CoursePrerequisite> CoursePrerequisites { get; set; } = new List<CoursePrerequisite>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}
