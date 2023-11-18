using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;

namespace MagicLand_System.Services.Implements
{
    public class CourseService : BaseService<CourseService>, ICourseService
    {
        public CourseService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<CourseService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<Course>> FilterCourseAsync(FilterCourseRequest request)
        {
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync();
            List<Course> filteredCourses;

            filteredCourses = string.IsNullOrEmpty(request.KeyWord)
                ? filteredCourses = courses.ToList()
                : filteredCourses = courses.Where(x => x.Name.ToLower().Contains(request.KeyWord.ToLower())).ToList();

            filteredCourses = request.MinYear != null
                ? filteredCourses.Where(x => x.MinYearStudent == request.MinYear).ToList()
                : filteredCourses;
            filteredCourses = request.MaxYear != null
                ? filteredCourses.Where(x => x.MaxYearStudent == request.MaxYear).ToList()
                : filteredCourses;
            filteredCourses = request.NumberSession != null
               ? filteredCourses.Where(x => x.NumberOfSession == request.NumberSession).ToList()
               : filteredCourses;


            return filteredCourses;

        }

        public Task<List<Course>> GetCourseByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Course>> GetCoursesAsync()
        {
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync();
            return courses.ToList();
        }

        public async Task<List<Course>> SearchCourseAsync(String keyWord)
        {
            var courses = string.IsNullOrEmpty(keyWord)
            ? await _unitOfWork.GetRepository<Course>().GetListAsync()
            : await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Name.ToLower().Contains(keyWord.ToLower()));
            return courses.ToList();
        }
    }
}
