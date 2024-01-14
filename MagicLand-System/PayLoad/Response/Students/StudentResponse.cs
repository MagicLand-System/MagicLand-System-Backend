namespace MagicLand_System.PayLoad.Response.Students
{
    public class StudentResponse
    {
        public Guid StudentId { get; set; }
        public required string FullName { get; set; }
        public required int Age { get; set; }
        public string? Gender { get; set; }
        public string? Avatar { get; set; }
    }
}
