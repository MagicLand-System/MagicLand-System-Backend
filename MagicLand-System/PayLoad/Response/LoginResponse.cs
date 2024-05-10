namespace MagicLand_System.PayLoad.Response
{
    public class LoginResponse
    {
        public Guid? UserId { get; set; }
        public string? FullName { set; get; }
        public required string Phone { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public required string Role { get; set; }
        public string AccessToken { get; set; }
    }
}
