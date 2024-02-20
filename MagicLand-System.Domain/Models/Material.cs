using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Material
    {
        public Guid Id { get; set; }
        public string URL { get; set; } 
        public Guid? CourseSyllabusId { get; set; }
        public CourseSyllabus? CourseSyllabus { get; set;}
    }
}
