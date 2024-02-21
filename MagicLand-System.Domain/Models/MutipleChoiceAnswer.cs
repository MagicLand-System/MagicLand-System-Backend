using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class MutipleChoiceAnswer
    {
        public Guid Id { get; set; }
        public string Description { get; set; }    
        public string? Img { get; set; } 
        public double Score { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }    

    }
}
