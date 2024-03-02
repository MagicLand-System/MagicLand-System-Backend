namespace MagicLand_System.PayLoad.Response.Carts
{
    public class CartForClassResponse
    {
        public Guid CartId { get; set; }
        public List<CartItemForClassResponse> Items { get; set; } = new List<CartItemForClassResponse>();

    }
}
