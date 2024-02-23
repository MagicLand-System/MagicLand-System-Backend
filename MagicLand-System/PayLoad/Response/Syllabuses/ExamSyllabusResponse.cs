namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class ExamSyllabusResponse
    {
        public string? Type { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double CompletionCriteria { get; set; }
        public string? Duration { get; set; } = string.Empty;
        public string? QuestionType { get; set; } = string.Empty;
        public int Part { get; set; }
    }
}
