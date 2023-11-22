using MagicLand_System.Domain.Models;

namespace MagicLand_System.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<IEnumerable<Promotion>> GetCurrentUserPromotion(); 
    }
}
