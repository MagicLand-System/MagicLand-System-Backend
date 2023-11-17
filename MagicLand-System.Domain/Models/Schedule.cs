using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Schedule
    {
        public Guid Id { get; set; }
        [ForeignKey("Class")]
        public Guid ClassId { get; set; }
        public Class Class { get; set; }    
        public int DayOfWeek { get; set; }
        public DateTime Date { get; set; }
        [ForeignKey("Slot")]
        public Guid SlotId { get; set; }
        public Slot Slot { get; set; }
        [ForeignKey("Room")]
        public Guid RoomId { get; set; }
        public Room Room { get; set; }
        public ICollection<ClassInstance> ClassInstances { get; set; }  

    }
}
