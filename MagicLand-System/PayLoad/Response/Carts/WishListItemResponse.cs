using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System.PayLoad.Response.Carts
{
    public class WishListItemResponse
    {
        public Guid CartItemId { get; set; }
        public required List<StudentResponse> Students { get; set; } = new List<StudentResponse>();
        public required ClassResExtraInfor Class { get; set; }
    }
}
