using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }
        [ForeignKey("Cart")]
        public Guid CartId { get; set; }
        public Cart Cart { get; set; }
        public Guid ClassId { get; set; }

        public ICollection<CartItemRelation> CartItemRelations { get; set; } = new List<CartItemRelation>();
    }
}
