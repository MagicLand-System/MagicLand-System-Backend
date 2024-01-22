using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }
        public string? TransactionCode {  get; set; }   
        public string? Signature { get; set; }
        public double Money { get; set; }
        public double Discount { get; set; } = 0.0;
        public string? Type { get; set; }
        public string? Method {  get; set; } 
        public string? Description {  get; set; }
        public string Currency { get; set; } = "VND";
        public string? Status { get; set; }
        public string? CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }


        [ForeignKey("PersonalWallet")]
        public Guid PersonalWalletId { get; set; }
        public PersonalWallet? PersonalWallet { get; set; }
    }
}
