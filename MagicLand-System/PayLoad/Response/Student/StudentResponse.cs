namespace MagicLand_System.PayLoad.Response.Student
{
    public class StudentResponse
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required int Age { get; set; }
        public string? Gender { get; set; }
        public string? Avatar { get; set; }
    }
}
