
namespace MagicLand_System.PayLoad.Request.Checkout
{
    public class CheckoutRequest
    {
        public required Guid ClassId { get; set; }
        public required List<Guid> StudentIdList { get; set; } = new List<Guid>();  
    }
}
