using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Attendance
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid ScheduleId { get; set; }
        public bool? IsPresent { get; set; }
        public Student Student { get; set; }
        public Schedule Schedule { get; set; }
    }
}
