namespace MagicLand_System.PayLoad.Request.Cart
{
    public class CartRequest
    {
        public required List<Guid> StudentIds { get; set; }
        public required Guid ClassId { get; set; }
    }
}
