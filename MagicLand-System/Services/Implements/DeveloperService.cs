using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;

namespace MagicLand_System.Services.Implements
{
    public class DeveloperService : BaseService<DeveloperService>, IDeveloperService
    {
        public DeveloperService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<DeveloperService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task GetDashboardRegisterResponses()
        {

        }
    }
}
