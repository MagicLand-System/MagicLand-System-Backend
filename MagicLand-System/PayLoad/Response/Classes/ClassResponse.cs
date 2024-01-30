namespace MagicLand_System.PayLoad.Response.Classes
{
    public class ClassResponse
    {
        public Guid ClassId { get; set; }
        public string? Name { get; set; }
        public string? ClassCode { get; set; }
        public string? ClassSubject { get; set; }
        public Guid CourseId { get; set; }
        public double? CoursePrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Address { get; set; } = "Home";
        public string? Status { get; set; }
        public string? Method { get; set; }
        public int LimitNumberStudent { get; set; }
        public int LeastNumberStudent { get; set; }
        public int NumberStudentRegistered { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
    }
}
