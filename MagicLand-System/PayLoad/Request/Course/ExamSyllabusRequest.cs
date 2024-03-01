namespace MagicLand_System.PayLoad.Request.Course
{
    public class ExamSyllabusRequest
    {
        public string Type { get; set; }
        public string ContentName { get; set; }
        public double Weight { get; set; }
        public double CompleteionCriteria { get; set; }
        public string QuestionType { get; set; }
        public int Part { get; set; }
        public string Method { get; set; }
        public string? Duration { get; set; }   
    }
}
