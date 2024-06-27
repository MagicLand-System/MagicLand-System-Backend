namespace MagicLand_System.Domain.Models
{
    public class Role
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }

        public ICollection<User> Accounts { get; set; } = new List<User>();

    }
}
