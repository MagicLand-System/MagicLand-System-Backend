namespace MagicLand_System.PayLoad.Response.Carts
{
    public class CartResponse
    {
        public Guid CartId { get; set; }

        public List<CartItemResponse> CartItems { get; set; } = new List<CartItemResponse>();
    }
}
