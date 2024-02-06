using AutoMapper;
using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Domain;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Services;

namespace MagicLand_System.Background.BackgroundServiceImplements
{
    public class ClassBackgroundService : BaseService<ClassBackgroundService>, IClassBackroundService
    {
        public ClassBackgroundService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<ClassBackgroundService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public string UpdateStatusClassInTimeAsync()
        {
            return "OK";
        }
    }
}
