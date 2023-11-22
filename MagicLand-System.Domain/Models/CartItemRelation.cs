using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class CartItemRelation
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }


        [ForeignKey("CartItem")]
        public Guid CartItemId { get; set; }
        public CartItem CartItem { get; set; }
    }
}
