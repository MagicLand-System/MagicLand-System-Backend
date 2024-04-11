namespace MagicLand_System.Domain.Models
{
    public class Room
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? Floor { get; set; }
        public string? Status { get; set; }
        public string? LinkURL { get; set; }
        public int Capacity { get; set; }
        public string? Type {  get; set; }   
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
