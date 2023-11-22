using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class PromotionService : BaseService<PromotionService>, IPromotionService
    {
        public PromotionService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<PromotionService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<Promotion>> GetCurrentUserPromotion() 
        {
            var userPromotions = await _unitOfWork.GetRepository<UserPromotion>().GetListAsync(
                predicate: x => x.UserId == GetUserIdFromJwt(),
                include: x => x.Include(x => x.Promotion));
            return userPromotions.Select(up => up.Promotion);
        }
    }
}
