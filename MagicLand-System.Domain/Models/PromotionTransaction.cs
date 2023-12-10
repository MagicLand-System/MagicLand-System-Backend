using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    //On Fixing
    public class PromotionTransaction
    {
        public Guid Id { get; set; }
        [ForeignKey("ClassFeeTransaction")]
        public Guid ClassFeeTransactionId { get; set; }

        [ForeignKey("UserPromotion")]
        public Guid UserPromotionId { get; set; }
        public UserPromotion UserPromotion { get; set; }  
    }
}
