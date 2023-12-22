
namespace MagicLand_System.PayLoad.Request.Checkout
{
    public class CheckoutRequest
    {
        public required List<Guid> StudentsIdList { get; set; }
        public required Guid ClassId { get; set; }
    }
}
