using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models.TempEntity.Quiz
{
    public class TempMCAnswer
    {
        public Guid Id { get; set; }
        public Guid AnswerId { get; set; }
        public double Score { get; set; }


        [ForeignKey("TempQuestion")]
        public Guid TempQuestionId { get; set; }
        public TempQuestion? TempQuestion { get; set; }

    }
}
