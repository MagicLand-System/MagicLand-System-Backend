namespace MagicLand_System.PayLoad.Response.Carts
{
    public class FavoriteResponse
    {
        public Guid CartId { get; set; }
        public List<FavoriteItemResponse> FavoriteItems { get; set; } = new List<FavoriteItemResponse>();
    }
}
