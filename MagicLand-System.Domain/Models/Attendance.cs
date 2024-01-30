using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Attendance
    {
        public Guid Id { get; set; }
        public bool? IsPresent { get; set; } = default;
        public bool? IsPublic { get; set; } = false;


        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public Student? Student { get; set; } 

        [ForeignKey("Schedule")]
        public Guid ScheduleId { get; set; }
        public Schedule? Schedule { get; set; }
    }
}
