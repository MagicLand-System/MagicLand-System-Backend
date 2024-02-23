using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class QuestionPackage
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }

        [ForeignKey("Session")]
        public Guid? SessionId { get; set; }
        public Session? Session { get; set; } = default;

        public List<Question>? Questions { get; set; }
    }
}
