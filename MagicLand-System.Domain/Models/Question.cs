using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class Question
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Img { get; set; }


        [ForeignKey("QuestionPackage")]
        public Guid QuestionPacketId { get; set; }
        public QuestionPackage? QuestionPackage { get; set; }

        public List<MultipleChoice>? MutipleChoices { get; set; }
        public List<FlashCard>? FlashCards { get; set; }
    }
}
