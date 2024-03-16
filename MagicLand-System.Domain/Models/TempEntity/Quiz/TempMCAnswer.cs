using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
