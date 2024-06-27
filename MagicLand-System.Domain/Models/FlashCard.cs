using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class FlashCard
    {
        public Guid Id { get; set; }
        public double Score { get; set; }

        [ForeignKey("Question")]
        public Guid QuestionId { get; set; }
        public Question? Question { get; set; }
        public List<SideFlashCard> SideFlashCards { get; set; } = new List<SideFlashCard>();
    }
}
