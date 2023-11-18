using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;

namespace MagicLand_System.Services.Implements
{
    public class ClassService : BaseService<ClassService>, IClassService
    {
        public ClassService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<ClassService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public Task<List<Class>> FilterClass()
        {
            throw new NotImplementedException();
        }

        public Task<Class> GetClassById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Class>> GetClassesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Class>> GetClassesByCourseIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Class>> SearchClass()
        {
            throw new NotImplementedException();
        }
    }
}
