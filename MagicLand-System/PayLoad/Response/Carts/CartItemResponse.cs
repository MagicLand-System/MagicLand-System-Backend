using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System.PayLoad.Response.Carts
{
    public class CartItemResponse
    {
        public Guid CartItemId { get; set; }
        public required string ItemType { get; set; }
        public Guid ItemId { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Code { get; set; } = string.Empty;
        public string? Subject { get; set; } = string.Empty;
        public double? Price { get; set; }
        public int MinYearOldStudent { get; set; }
        public int MaxYearOldStudent { get; set; }
        public string? Image { get; set; } = string.Empty;
    }
}
