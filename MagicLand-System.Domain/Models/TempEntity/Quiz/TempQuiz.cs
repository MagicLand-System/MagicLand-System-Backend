namespace MagicLand_System.Domain.Models.TempEntity.Quiz
{
    public class TempQuiz
    {
        public Guid Id { get; set; }
        public Guid ExamId { get; set; }
        public Guid StudentId { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public bool IsGraded { get; set; } = false;
        public string? ExamType { get; set; }
        public int TotalMark { get; set; }

        public ICollection<TempQuestion> Questions { get; set; } = new List<TempQuestion>();
    }
}
