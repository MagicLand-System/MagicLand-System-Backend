using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.WalletTransactions;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class PersonalWalletService : BaseService<PersonalWallet>, IPersonalWalletService
    {
        public PersonalWalletService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<PersonalWallet> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<WalletResponse> GetWalletOfCurrentUser()
        {
            var id = (await GetUserFromJwt()).Id;
            var currentUser = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id.ToString()),include: x => x.Include(x => x.PersonalWallet));
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
