using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }
        public double Money { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool? IsProcessed { get; set; }


        [ForeignKey("PersonalWallet")]
        public Guid PersonalWalletId { get; set; }
        public PersonalWallet PersonalWallet { get; set; } = new PersonalWallet();
    }
}
