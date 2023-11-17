using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class ClassInstance
    {
        public Guid Id { get; set; }
        [ForeignKey("Session")]    
        public Guid SessionId { get; set; }    
        public Session Session { get; set; }
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public Student Student { get; set; }
        public string Status { get; set; }
    }
}
