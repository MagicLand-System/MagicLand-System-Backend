using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class QuestionPackage
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string ContentName { get; set; } = string.Empty;
        public string QuizType { get; set; } = string.Empty;
        public string PackageType { get; set; } = string.Empty;
        public int OrderPackage { get; set; }
        public int Score { get; set; }
        public int NoSession { get; set; }


        [ForeignKey("Session")]
        public Guid SessionId { get; set; }
        public Session? Session { get; set; }

        public List<Question>? Questions { get; set; }
    }
}
