using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;

namespace MagicLand_System.Services.Implements
{
    public class UserService : BaseService<UserService>, IUserService
    {
        public UserService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<UserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<User>> GetUsers()
        {
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(predicate : x => x.Id == x.Id);
            return users.ToList();
        }
    }
}
