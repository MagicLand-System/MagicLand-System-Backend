using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models.TempEntity
{
    public class TempQuestion
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }


        [ForeignKey("TempQuiz")]
        public Guid TempQuizId { get; set; }
        public TempQuiz? TempQuiz { get; set; }


        public ICollection<TempMCAnswer> MCAnswers { get; set; } = new List<TempMCAnswer>();
        public ICollection<TempFCAnswer> FCAnswers { get; set; } = new List<TempFCAnswer>();
    }
}
