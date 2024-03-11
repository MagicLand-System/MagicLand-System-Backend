using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class ExamQuestion
    {
        public Guid Id { get; set; }
        public string? Question { get; set; } = string.Empty;
        public string? QuestionImage { get; set; } = string.Empty;
        public string? Status { get; set; }
        public double Score { get; set; }

        [ForeignKey("TestResult")]
        public Guid TestResultId { get; set; }
        public TestResult? TestResult { get; set; }

        public ICollection<MultipleChoiceAnswer>? MultipleChoiceAnswers { get; set; }
        public ICollection<FlashCardAnswer>? FlashCardAnswers { get; set; }
    }
}
