using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid CourseId { get; set; } = default;


        [ForeignKey("Cart")]
        public Guid CartId { get; set; }
        public Cart? Cart { get; set; }

        public ICollection<StudentInCart> StudentInCarts { get; set; } = new List<StudentInCart>();
    }
}
