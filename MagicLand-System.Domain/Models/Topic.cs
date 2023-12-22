using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int OrderNumber {  get; set; }   
        public Guid CourseSyllabusId { get; set; }
        public CourseSyllabus CourseSyllabus { get; set; }
        public ICollection<Session> Sessions = new List<Session>();
    }
}
