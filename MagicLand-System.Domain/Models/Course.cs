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
        public int MinYearStudent { get; set; } = 3;
        public int MaxYearStudent { get; set; }
        public string Status { get; set; }
        public ICollection<CoursePrerequisite> coursePrerequisites { get; set; } = new List<CoursePrerequisite>();
    }
}
