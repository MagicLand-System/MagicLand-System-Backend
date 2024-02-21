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
        public Guid? CourseId { get; set; } = null;
        public Course? Course { get; set; }
        public ICollection<Topic> Topics { get; set; } = new List<Topic>();
        public ICollection<Material> Materials { get; set; } = new List<Material>();
    }
}
