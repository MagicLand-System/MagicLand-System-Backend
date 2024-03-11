using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class MultipleChoice
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Img { get; set; }
        public double Score { get; set; }


        [ForeignKey("Question")]
        public Guid QuestionId { get; set; }
        public Question? Question { get; set; }

    }
}
