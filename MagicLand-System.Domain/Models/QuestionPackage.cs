using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class QuestionPackage
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public int? OrderPackage { get; set; }
        public string? Type { get; set; }
        public int? Duration { get; set; }
        public int? DeadlineTime { get; set; }
        //public DateTime StartTime { get; set; }
        //public DateTime EndTime { get; set; }
        public int? Score { get; set; }
        public string? ContentName { get; set; }
        public int? AttemptsAllowed { get; set; }
        public int? NoSession { get; set; }


        [ForeignKey("Session")]
        public Guid? SessionId { get; set; }
        public Session? Session { get; set; } = default;

        public List<Question>? Questions { get; set; }
    }
}
