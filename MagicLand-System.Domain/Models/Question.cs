using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public List<MutipleChoiceAnswer>? MutipleChoiceAnswers { get; set; }
        public List<FlashCard>? FlashCards { get; set; }
    }
}
