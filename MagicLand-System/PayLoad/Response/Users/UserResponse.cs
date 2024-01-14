using MagicLand_System.PayLoad.Response.Addresses;

namespace MagicLand_System.PayLoad.Response.Users
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarImage { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }

    }
}
