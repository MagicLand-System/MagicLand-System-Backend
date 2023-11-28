using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }
        public double Money { get; set; }  
        public string? Type { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedTime { get; set; }

        [ForeignKey("PersonalWallet")]
        public Guid PersonalWalletId { get; set; }
        public required PersonalWallet PersonalWallet { get; set; }
    }
}
