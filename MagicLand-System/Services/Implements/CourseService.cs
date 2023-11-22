using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.CustomMapper;
using MagicLand_System.PayLoad.Response.Course;
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

        public async Task<List<CourseResponse>> FilterCourseAsync(string? keyWord, int? minYearsOld, int? maxYearsOld, int? numberOfSession)
        {

            minYearsOld ??= 0;
            maxYearsOld ??= 150;

            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(include: x => x.Include(x => x.CoursePrerequisites));

            var filteredCourses = string.IsNullOrEmpty(keyWord)
                ? courses.ToList()
                : courses.Where(x => x.Name.ToLower().Contains(keyWord.ToLower())).ToList();


            filteredCourses = minYearsOld > maxYearsOld || minYearsOld < 0 || maxYearsOld < 0
            ? throw new BadHttpRequestException("Range Of Age Not Valid", StatusCodes.Status400BadRequest)
            : filteredCourses.Where(x => x.MinYearOldsStudent >= minYearsOld && x.MaxYearOldsStudent <= maxYearsOld).ToList();

            filteredCourses = numberOfSession != null
            ? filteredCourses.Where(x => x.NumberOfSession == numberOfSession).ToList()
            : filteredCourses;

            Course[] coursePrerequisiteFiltereds = await GetCoursePrerequesites(filteredCourses);

            return filteredCourses.Select(fc => CustomMapper
                   .fromCourseToCourseResponse(fc, coursePrerequisiteFiltereds
                  .Where(cpf => fc.CoursePrerequisites.Any(cp => cp.PrerequisiteCourseId == cpf.Id)))).ToList();
        }

        public async Task<CourseResponse> GetCourseByIdAsync(Guid id)
        {
            var course = await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Id == id, include: x => x.Include(x => x.CoursePrerequisites));

            var coursePrerequisites = course == null
                ? throw new BadHttpRequestException("Id Not Exist", StatusCodes.Status400BadRequest)
                : await GetCoursePrerequesites(course);

            return CustomMapper.fromCourseToCourseResponse(course.ToList()[0], coursePrerequisites);
        }

        public async Task<List<CourseResponse>> GetCoursesAsync()
        {
            var courses = await _unitOfWork.GetRepository<Course>()
                .GetListAsync(include: x => x.Include(x => x.CoursePrerequisites));

            Course[] coursePrerequisites = await GetCoursePrerequesites(courses);

            return courses.Select(c => CustomMapper
            .fromCourseToCourseResponse(c, coursePrerequisites
            .Where(cp => c.CoursePrerequisites.Any(x => x.PrerequisiteCourseId == cp.Id)))).ToList();
        }


        public async Task<List<CourseResponse>> SearchCourseAsync(String keyWord)
        {
            var courses = string.IsNullOrEmpty(keyWord)
            ? await _unitOfWork.GetRepository<Course>().GetListAsync(include: x => x.Include(x => x.CoursePrerequisites))
            : await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Name.ToLower().Contains(keyWord.ToLower()), include: x => x.Include(x => x.CoursePrerequisites));

            Course[] coursePrerequisites = await GetCoursePrerequesites(courses);

            return courses.Select(c => CustomMapper
                  .fromCourseToCourseResponse(c, coursePrerequisites
                  .Where(cp => c.CoursePrerequisites.Any(x => x.PrerequisiteCourseId == cp.Id)))).ToList();
        }

        private async Task<Course[]> GetCoursePrerequesites(ICollection<Course> courses)
        {
            var coursePrerequesites = new List<Course>();

            foreach (var cp in courses.SelectMany(c => c.CoursePrerequisites))
            {
                var course = await _unitOfWork.GetRepository<Course>()
                    .SingleOrDefaultAsync(predicate: c => c.Id == cp.PrerequisiteCourseId);

                coursePrerequesites.Add(course);
            }

            return coursePrerequesites.ToArray();
        }

    }
}
