namespace MagicLand_System.PayLoad.Response.Students
{
    public class StudentWithAccountResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? AvatarImage { get; set; }
        public DateTime AddedTime { get; set; }
        public bool IsActive { get; set; }
        public Guid ParentId { get; set; }
        public required string StudentAccount { get; set; }
    }
}
