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
        [ForeignKey("Schedule")]    
        public Guid ScheduleId { get; set; }    
        public Schedule Schedule { get; set; }
        [ForeignKey("User")]
        public Guid StudentId { get; set; }
        public User User { get; set; }
        public string Status { get; set; }
    }
}
