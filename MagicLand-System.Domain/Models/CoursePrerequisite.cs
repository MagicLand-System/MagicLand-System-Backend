using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class CoursePrerequisite
    {
        public Guid Id { get; set; }
        [ForeignKey("Course")]
        public Guid CurrentCourseId { get; set; }
        public Course Course { get; set; }
        public Guid PrerequisiteCourseId { get; set; }
    }
}
