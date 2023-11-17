using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class UserPromotion
    {
        public Guid Id { get; set; }
        [ForeignKey("Promotion")]
        public Guid PromotionId { get; set; }
        public Promotion Promotion { get; set; }
        public int? AccumulateQuantity { get; set; } = null;
        [ForeignKey("User")]
        public Guid UserId { get; set; }    
        public User User { get; set; }
        public ICollection<PromotionTransaction> PromotionTransactions { get; set; } = new List<PromotionTransaction>();
    }
}
