using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    //On Fixing
    public class Promotion
    {
        public Guid Id { get; set; }
        public string Code { get; set; }    
        public string Image { get; set; }   
        public string UnitDiscount { get; set; }
        public int DiscountValue { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<UserPromotion> UserPromotions { get; set; } = new List<UserPromotion>();

    }
}
