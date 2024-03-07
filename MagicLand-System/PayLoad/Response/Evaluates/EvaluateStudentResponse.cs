namespace MagicLand_System.PayLoad.Response.Evaluates
{
    public class EvaluateStudentResponse
    {
        public required Guid StudentId { get; set; }
        public required string StudentName { get; set; }
        public required int Level { get; set; }
        public required string EvaluateDescription { get; set; }
        public string? Note { get; set; } = string.Empty;
    }
}
