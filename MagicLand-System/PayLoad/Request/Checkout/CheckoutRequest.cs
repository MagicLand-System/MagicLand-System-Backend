namespace MagicLand_System.PayLoad.Request.Checkout
{
    public class CheckoutRequest
    {
        public List<Guid> StudentsIdList { get; set; }
        public List<Guid>? UserPromotions { get; set; } = null;
        public Guid ClassId { get; set; }
    }
}
