using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.CustomMapper;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MagicLand_System.Services.Implements
{
    public class ClassService : BaseService<ClassService>, IClassService
    {
        public ClassService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<ClassService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<ClassResponse>> FilterClassAsync(List<string>? keyWords, double? minPrice, double? maxPrice, int? limitStudent)
        {
            var classes = await FetchClasses();

            //For satisfy all key word

            //classes = keyWords == null || keyWords.Count() == 0
            //        ? classes
            //        : classes.Where(c => keyWords.All(k =>
            //        k == null ||
            //       c.Name.ToLower().Contains(k.ToLower()) ||
            //       c.StartDate.ToString().ToLower().Contains(k.ToLower()) ||
            //       c.EndDate.ToString().ToLower().Contains(k.ToLower()) ||
            //       c.Method.ToString().ToLower().Contains(k.ToLower()) || 
            //       c.Status.ToString().ToLower().Contains(k.ToLower()) ||
            //       (c.Address != null && (c.Address.City!.ToLower().Contains(k.ToLower()) || 
            //       c.Address.Street!.ToLower().Contains(k.ToLower()) || 
            //       c.Address.District!.ToLower().Contains(k.ToLower()))))).ToList();

            //For satisfy just one key word
            //classes = keyWords == null || keyWords.Count() == 0
            //        ? classes
            //        : classes.Where(c => keyWords.Any(k =>
            //        (k != null) &&
            //        (c.Name.ToLower().Contains(k.ToLower()) ||
            //        c.StartTime.ToString().ToLower().Contains(k.ToLower()) ||
            //        c.EndTime.ToString().ToLower().Contains(k.ToLower()) ||
            //        c.Method.ToString().ToLower().Contains(k.ToLower()) ||
            //        (c.Address != null && c.Address.ToString()!.ToLower().Contains(k.ToLower()))))).ToList();

            minPrice ??= 0;
            maxPrice ??= double.MaxValue;

            classes = minPrice < 0 || maxPrice < 0 || minPrice > maxPrice
                ? throw new BadHttpRequestException("Range Of Price Not Valid", StatusCodes.Status400BadRequest)
                : classes.Where(c => limitStudent != null
                ? c.Price >= minPrice && c.Price <= maxPrice && c.LimitNumberStudent == limitStudent
                : c.Price >= minPrice && c.Price <= maxPrice).ToList();

            return classes.Select(c => _mapper.Map<ClassResponse>(c)).ToList();
        }

        public async Task<ClassResponse> GetClassByIdAsync(Guid id)
        {

            var cls = await _unitOfWork.GetRepository<Class>()
               .SingleOrDefaultAsync(predicate: x => x.Id == id);

            return _mapper.Map<ClassResponse>(cls);
        }

        public async Task<List<ClassResponse>> GetClassesAsync()
        {
            var classes = await FetchClasses();

            return classes.Select(c => _mapper.Map<ClassResponse>(c)).ToList();
        }

        public async Task<List<ClassResponse>> GetClassesByCourseIdAsync(Guid id)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == id);

            var classes = course == null
                ? throw new BadHttpRequestException("Course Id Not Exist", StatusCodes.Status400BadRequest)
                : await _unitOfWork.GetRepository<Class>()
                .GetListAsync(predicate: x => x.CourseId == id);

            return classes.Select(c => _mapper.Map<ClassResponse>(c)).ToList();
        }


        private async Task<ICollection<Class>> FetchClasses()
        {
            return await _unitOfWork.GetRepository<Class>()
                .GetListAsync();

        }
    }
}
