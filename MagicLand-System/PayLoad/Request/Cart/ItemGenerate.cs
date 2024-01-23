namespace MagicLand_System.PayLoad.Request.Cart
{
    public class ItemGenerate
    {
        public Guid? CartItemId { get; set; } = default;
        public required Guid ClassId { get; set; }
        public required List<Guid> StudentIdList { get; set;}
    }
}
