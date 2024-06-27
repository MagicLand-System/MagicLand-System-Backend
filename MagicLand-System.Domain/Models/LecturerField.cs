namespace MagicLand_System.Domain.Models
{
    public class LecturerField
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
