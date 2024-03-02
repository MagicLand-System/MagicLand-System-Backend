namespace MagicLand_System.PayLoad.Response.Carts
{
    public class CartForCourseResponse
    {
        public Guid CartId { get; set; }
        public List<CartItemForCourseResponse> Items { get; set; } = new List<CartItemForCourseResponse>();
    }
}
