using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int OrderNumber {  get; set; }

        [ForeignKey("CourseSyllabus")]
        public Guid CourseSyllabusId { get; set; }
        public CourseSyllabus CourseSyllabus { get; set; } = new CourseSyllabus();

        public ICollection<Session> Sessions = new List<Session>();
    }
}
