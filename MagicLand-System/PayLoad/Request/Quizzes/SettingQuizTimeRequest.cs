namespace MagicLand_System.PayLoad.Request.Quizzes
{
    public class SettingQuizTimeRequest
    {
        public TimeOnly QuizStartTime { get; set; } = default;
        public TimeOnly QuizEndTime { get; set; } = default;
        public int? AttemptAllowed { get; set; } = 1;
        public int? Duration { get; set; } = 600;
    }
}
