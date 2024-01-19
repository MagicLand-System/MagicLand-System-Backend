using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.WalletTransactions;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;

namespace MagicLand_System.Services.Implements
{
    public class PersonalWalletService : BaseService<PersonalWallet>, IPersonalWalletService
    {
        public PersonalWalletService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<PersonalWallet> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<WalletResponse> GetWalletOfCurrentUser()
        {
            var currentUser = await GetUserFromJwt();
            if (currentUser == null)
            {
                return new WalletResponse();
            }
            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate : x => x.UserId.ToString().Equals(currentUser.Id.ToString()));
            if (personalWallet == null)
            {
                return new WalletResponse();
            }
            return new WalletResponse
            {
                Balance = personalWallet.Balance,
                Owner = currentUser.FullName,
            };
        }
    }
}
