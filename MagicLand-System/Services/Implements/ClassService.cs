using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.CustomMapper;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class ClassService : BaseService<ClassService>, IClassService
    {
        public ClassService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<ClassService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public Task<List<ClassResponse>> FilterClass()
        {
            throw new NotImplementedException();
        }

        public async Task<ClassResponse> GetClassById(Guid id)
        {
            var cls = await _unitOfWork.GetRepository<Class>()
               .SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x
               .Include(x => x.ClasssTransactions)
               .Include(x => x.User).ThenInclude(u => u.Address)
               .Include(x => x.Address)!
               .Include(x => x.ClasssTransactions));

            return _mapper.Map<ClassResponse>(cls);
        }

        public async Task<List<ClassResponse>> GetClassesAsync()
        {
            var classes = await _unitOfWork.GetRepository<Class>()
                .GetListAsync(include: x => x
               .Include(x => x.ClasssTransactions)
               .Include(x => x.User).ThenInclude(u => u.Address)
               .Include(x => x.Address)!
               .Include(x => x.ClasssTransactions));

            return classes.Select(c => _mapper.Map<ClassResponse>(c)).ToList();
        }

        public async Task<List<ClassResponse>> GetClassesByCourseIdAsync(Guid id)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == id);

            var classes = course == null
                ? throw new BadHttpRequestException("Course Id Not Exist", StatusCodes.Status400BadRequest)
                : await _unitOfWork.GetRepository<Class>()
                .GetListAsync(predicate: x => x.CourseId == id, include: x => x
                .Include(x => x.ClasssTransactions)
                .Include(x => x.User).ThenInclude(u => u.Address)
                .Include(x => x.Address)!
                .Include(x => x.ClasssTransactions));

            return classes.Select(c => _mapper.Map<ClassResponse>(c)).ToList();
        }

        public Task<List<ClassResponse>> SearchClass()
        {
            throw new NotImplementedException();
        }
    }
}
