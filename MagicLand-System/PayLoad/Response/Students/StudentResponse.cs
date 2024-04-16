namespace MagicLand_System.PayLoad.Response.Students
{
    public class StudentResponse
    {
        public Guid StudentId { get; set; }
        public required string FullName { get; set; }
        public required int Age { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? AvatarImage { get; set; }
        public string? Email { get; set; }
        public bool? CanRegistered { get; set; }    
        public string? ReasonCannotRegistered {  get; set; }    
    }
}
