using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Users;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MagicLand_System.PayLoad.Response.WalletTransactions
{
    public class WalletTransactionResponse
    {
        public UserResponse Parent {  get; set; } 
        public Guid TransactionId { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public string Type {  get; set; }
        public string? Description { get; set; }
        public DateTime CreatedTime { get; set; }
        public double Money { get; set; }
        public Domain.Models.Class MyClassResponse { get; set; }
        public string CourseName {  get; set; } 
        public string Method {  get; set; } 
        public string? TransactionCode {  get; set; }   
        public string Status { get; set; }
        public string Currency { get; set; }
        public string? CreateBy { get; set; }
        public string? Signature { get; set; }
        public double Discount { get; set; } = 0.0;

    }
}
