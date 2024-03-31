namespace MagicLand_System.PayLoad.Response.Carts
{
    public class WishListResponse
    {
        public Guid CartId { get; set; }
        public List<WishListItemResponse> Items { get; set; } = new List<WishListItemResponse>();
    }
}
