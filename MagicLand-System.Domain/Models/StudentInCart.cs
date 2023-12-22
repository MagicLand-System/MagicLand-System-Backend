using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class StudentInCart
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }


        [ForeignKey("CartItem")]
        public Guid CartItemId { get; set; }
        public CartItem? CartItem { get; set; }
    }
}
