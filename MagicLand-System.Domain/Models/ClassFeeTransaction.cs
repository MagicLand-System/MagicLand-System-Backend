using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    // Delete If Need 
    public class ClassFeeTransaction
    {
        public Guid Id { get; set; }
        [ForeignKey("User")]
        //public Guid ParentId { get; set; }  
        //public User User { get; set; }  
        public DateTime DateCreated { get; set; }   
        public double? ActualPrice { get; set; }
        public ICollection<PromotionTransaction> PromotionTransactions { get; set; } = new List<PromotionTransaction>();
        public ICollection<ClassTransaction> ClassTransactions { get; set; } = new List<ClassTransaction>();
    }
}
