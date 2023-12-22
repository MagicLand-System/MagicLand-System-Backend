using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class Schedule
    {
        public Guid Id { get; set; }
        public int DayOfWeek { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey("Class")]
        public Guid ClassId { get; set; }
        public Class Class { get; set; }

        [ForeignKey("Slot")]
        public Guid SlotId { get; set; }
        public Slot? Slot { get; set; }

        [ForeignKey("Room")]
        public Guid RoomId { get; set; }
        public Room? Room { get; set; }

    }
}
