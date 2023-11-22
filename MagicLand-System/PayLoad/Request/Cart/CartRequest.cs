namespace MagicLand_System.PayLoad.Request.Cart
{
    public class CartRequest
    {
        public required List<Guid> StudentId { get; set; }
        public required Guid ClassId { get; set; }
    }
}
