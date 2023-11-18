using AutoMapper;
using Azure.Core;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class CourseService : BaseService<CourseService>, ICourseService
    {
        public CourseService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<CourseService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<Course>> FilterCourseAsync(int minYearsOld , string? keyWord = null, int? maxYearsOld = null, int? numberOfSession = null)
        {
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(include: x => x.Include(x => x.CoursePrerequisites));
            List<Course> filteredCourses = new List<Course>();
            if(maxYearsOld == null)
            {
                maxYearsOld = 130;
            }
            if (string.IsNullOrEmpty(keyWord))
            {
                filteredCourses = courses.Where(x => (x.MinYearOldsStudent >= minYearsOld &&  x.MaxYearOldsStudent <= maxYearsOld)).ToList();
             
            } else
            {
                filteredCourses = courses.Where(x => (x.MinYearOldsStudent >= minYearsOld && x.MaxYearOldsStudent <= maxYearsOld && x.Name.ToLower().Contains(keyWord.ToLower()))).ToList();
            }         
            //filteredCourses = numberOfSession != null
            //   ? filteredCourses.Where(x => x.NumberOfSession == numberOfSession).ToList()
            //   : filteredCourses;
            return filteredCourses;
        }

        public Task<List<Course>> GetCourseByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Course>> GetCoursesAsync()
        {
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(include : x => x.Include(x => x.CoursePrerequisites));
            return courses.ToList();
        }

        public async Task<List<Course>> SearchCourseAsync(String keyWord)
        {
            var courses = string.IsNullOrEmpty(keyWord)
            ? await _unitOfWork.GetRepository<Course>().GetListAsync(include: x => x.Include(x => x.CoursePrerequisites))
            : await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Name.ToLower().Contains(keyWord.ToLower()), include: x => x.Include(x => x.CoursePrerequisites));
            return courses.ToList();
        }
    }
}
