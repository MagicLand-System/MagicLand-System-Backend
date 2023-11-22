using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Cart;

namespace MagicLand_System.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponse> ModifyCartOffCurrentParentAsync(List<Guid> studentIds, Guid classId);
        Task<CartResponse> GetCartOfCurrentParentAsync();
        Task<bool> DeleteItemInCartOfCurrentParentAsync(Guid itemId);
    }
}
