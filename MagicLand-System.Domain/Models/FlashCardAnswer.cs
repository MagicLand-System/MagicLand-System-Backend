using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class FlashCardAnswer
    {
        public Guid Id { get; set; }
        public Guid LeftCardAnswerId { get; set; }
        public string? LeftCardAnswer { get; set; } = string.Empty;
        public string? LeftCardAnswerImage { get; set; } = string.Empty;
        public Guid RightCardAnswerId { get; set; }
        public string? RightCardAnswer { get; set; } = string.Empty;
        public string? RightCardAnswerImage { get; set; } = string.Empty;
        public Guid CorrectRightCardAnswerId { get; set; }
        public string? CorrectRightCardAnswer { get; set; } = string.Empty;
        public string? CorrectRightCardAnswerImage { get; set; } = string.Empty;
        public string? Status { get; set; }
        public double Score { get; set; }

        [ForeignKey("ExamQuestion")]
        public Guid ExamQuestionId { get; set; }
        public ExamQuestion? ExamQuestion { get; set; }
    }
}
