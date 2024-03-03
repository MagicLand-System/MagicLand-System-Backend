using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Carts.GeneralCart;

namespace MagicLand_System.Services.Interfaces
{
    public interface ICartService
    {
        Task<WishListResponse> ModifyCartOffCurrentParentAsync(List<Guid> studentIds, Guid classId);
        Task<FavoriteResponse> AddCourseFavoriteOffCurrentParentAsync(Guid courseId);
        Task<WishListResponse> GetDetailCurrentParrentCart();
        Task<FavoriteResponse> GetDetailCurrentParrentFavorite();
        Task<CartResponse> GetAllItemsInCartAsync();
        Task DeleteItemInCartOfCurrentParentAsync(List<Guid> itemIds);
    }
}
