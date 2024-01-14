using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Carts;

namespace MagicLand_System.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponse> ModifyCartOffCurrentParentAsync(List<Guid> studentIds, Guid classId);
        Task<FavoriteResponse> AddCourseFavoriteOffCurrentParentAsync(Guid courseId);
        Task<CartResponse> GetDetailCurrentParrentCart();
        Task<FavoriteResponse> GetDetailCurrentParrentFavorite();
        Task DeleteItemInCartOfCurrentParentAsync(List<Guid> itemIds);
    }
}
