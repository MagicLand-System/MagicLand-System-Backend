namespace MagicLand_System.Domain.Models
{
    public class Slot
    {
        public Guid Id { get; set; }
        public required string StartTime { get; set; }
        public required string EndTime { get; set; }


        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
