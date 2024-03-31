namespace MagicLand_System.PayLoad.Response.Users
{
    public class AccountStudentResponse
    {
        public Guid AccountId { get; set; }
        public required string StudentName { get; set; } = string.Empty;
        public required string AccountPhone { get; set; }
        //public string? AvatarImage { get; set; } = string.Empty;
        //public string? Email { get; set; } = string.Empty;
        //public string? Gender { get; set; } = string.Empty;
        //public required DateTime DateOfBirth { get; set; }
        //public string? Address { get; set; }
    }
}
