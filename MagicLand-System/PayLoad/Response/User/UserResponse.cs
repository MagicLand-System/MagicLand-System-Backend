using MagicLand_System.PayLoad.Response.Address;

namespace MagicLand_System.PayLoad.Response.User
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


        public AddressResponse? Address { get; set; }
    }
}
