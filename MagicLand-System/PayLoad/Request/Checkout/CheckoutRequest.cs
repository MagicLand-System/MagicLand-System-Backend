
namespace MagicLand_System.PayLoad.Request.Checkout
{
    public class CheckoutRequest
    {
        public required List<Guid> StudentIdList { get; set; }
        public required Guid ClassId { get; set; }
    }
}
