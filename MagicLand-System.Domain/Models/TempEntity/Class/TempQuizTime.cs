namespace MagicLand_System.Domain.Models.TempEntity.Class
{
    public class TempQuizTime
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        public Guid ExamId { get; set; }
        public TimeSpan ExamStartTime { get; set; }
        public TimeSpan ExamEndTime { get; set; }
        public int Duration { get; set; }
        public int AttemptAllowed { get; set; }
    }
}
