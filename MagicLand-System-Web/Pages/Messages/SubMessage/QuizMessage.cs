namespace MagicLand_System_Web_Dev.Pages.Messages.SubMessage
{
    public class QuizMessage
    {
        public string QuizName { get; set; } = string.Empty;
        public int NoAttempt { get; set; }
        public int TotalMark { get; set; } = 0;
        public int CorrectMark { get; set; }
        public double TotalScore { get; set; }
        public double ScoreEarned { get; set; }
        public string ExamStatus { get; set; } = string.Empty;
    }
}
