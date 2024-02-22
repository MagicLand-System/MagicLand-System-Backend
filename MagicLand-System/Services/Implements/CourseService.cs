using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace MagicLand_System.Services.Implements
{
    public class CourseService : BaseService<CourseService>, ICourseService
    {
        public CourseService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<CourseService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<CourseResExtraInfor>> FilterCourseAsync(
            int minYearsOld,
            int maxYearsOld,
            int? minNumberSession,
            int? maxNumberSession,
            double minPrice,
            double? maxPrice,
            string? subject,
            int? rate)
        {

            var courses = await GetDefaultCourse();

            var filteredCourses = FilterProgress(minYearsOld, maxYearsOld, minNumberSession, maxNumberSession, minPrice, maxPrice, subject, courses);

            var coursePrerequisitesFilter = await GetCoursePrerequesites(filteredCourses);
            var coureSubsequentsFilter = await GetCoureSubsequents(filteredCourses);

            return filteredCourses.Select(fc => CourseCustomMapper
                   .fromCourseToCourseResExtraInfor(fc, coursePrerequisitesFilter
                   .Where(cpf => fc.CoursePrerequisites.Any(cp => cp.PrerequisiteCourseId == cpf.Id)),
                   coureSubsequentsFilter)).ToList();
        }

        private List<Course> FilterProgress(int minYearsOld, int maxYearsOld, int? minNumberSession, int? maxNumberSession, double minPrice, double? maxPrice, string? subject, ICollection<Course> courses)
        {
            maxNumberSession ??= int.MaxValue;
            maxPrice ??= double.MaxValue;

            var filteredCourses = minYearsOld > maxYearsOld || minYearsOld < 0 || maxYearsOld < 0
             ? throw new BadHttpRequestException("Độ Tuổi Truy Suất Không Hợp Lệ", StatusCodes.Status400BadRequest)
             : courses.Where(x => x.MinYearOldsStudent >= minYearsOld && x.MaxYearOldsStudent <= maxYearsOld).ToList();

            filteredCourses = minPrice > maxPrice || minPrice < 0 || maxPrice < 0
            ? throw new BadHttpRequestException("Gía Cả Truy Suất Không Hợp Lệ", StatusCodes.Status400BadRequest)
            : filteredCourses.Where(x => x.Price >= minPrice && x.Price <= maxPrice).ToList();

            filteredCourses = filteredCourses.Where(x => x.NumberOfSession >= minNumberSession && x.NumberOfSession <= maxNumberSession).ToList();

            //filteredCourses = subject != null
            ////? filteredCourses.Where(x => x.CourseCategory!.Name!.ToLower().Equals(subject.ToLower())).ToList()
            //: filteredCourses;

            return filteredCourses;
        }

        public async Task<CourseResExtraInfor> GetCourseByIdAsync(Guid id)
        {
            var course = await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Id == id, include: x => x
            .Include(x => x.CoursePrerequisites)
            //.Include(x => x.CourseCategory)
            .Include(x => x.Classes)
            .ThenInclude(c => c.Schedules)
            .ThenInclude(s => s.Slot)
            .Include(x => x.SubDescriptionTitles)
            .ThenInclude(sdt => sdt.SubDescriptionContents)
            .Include(x => x.CourseSyllabus)
            .ThenInclude(cs => cs!.Topics.OrderBy(tp => tp.OrderNumber))
            .ThenInclude(tp => tp.Sessions.OrderBy(s => s.NoSession)));

            var coursePrerequisites = !course.Any()
                ? throw new BadHttpRequestException($"Id [{id}] Khóa Hoc Không Tồn Tại", StatusCodes.Status400BadRequest)
                : await GetCoursePrerequesites(course);

            var coureSubsequents = await GetCoureSubsequents(course);

            return CourseCustomMapper.fromCourseToCourseResExtraInfor(course.ToList()[0], coursePrerequisites, coureSubsequents);
        }

        public async Task<List<CourseCategory>> GetCourseCategories()
        {
            var categories = await _unitOfWork.GetRepository<CourseCategory>().GetListAsync();
            return categories.ToList();
        }

        public async Task<List<CourseResExtraInfor>> GetCoursesAsync()
        {
            var courses = await GetDefaultCourse();

            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

            return courses.Select(c => CourseCustomMapper
            .fromCourseToCourseResExtraInfor(c, coursePrerequisites
            .Where(cp => c.CoursePrerequisites.Any(x => x.PrerequisiteCourseId == cp.Id)),
            coureSubsequents)).ToList();
        }


        public async Task<List<CourseResExtraInfor>> SearchCourseByNameOrAddedDateAsync(string keyWord)
        {
            var courses = string.IsNullOrEmpty(keyWord)
            ? await GetDefaultCourse()
            : await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Name!.ToLower().Contains(keyWord.ToLower()), include: x => x
            .Include(x => x.CoursePrerequisites)
            //.Include(x => x.CourseCategory)
            .Include(x => x.Classes)
            .ThenInclude(c => c.Schedules)
            .ThenInclude(s => s.Slot)
            .Include(x => x.SubDescriptionTitles)
            .ThenInclude(sdt => sdt.SubDescriptionContents)
            .Include(x => x.CourseSyllabus)
            .ThenInclude(cs => cs!.Topics.OrderBy(tp => tp.OrderNumber))
            .ThenInclude(tp => tp.Sessions.OrderBy(s => s.NoSession)));

            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

            var findCourse =  courses.Select(c => CourseCustomMapper
                  .fromCourseToCourseResExtraInfor(c, coursePrerequisites
                  .Where(cp => c.CoursePrerequisites.Any(x => x.PrerequisiteCourseId == cp.Id)),
                  coureSubsequents)).ToList();
            foreach (var course in findCourse)
            {
                var count = (await _unitOfWork.GetRepository<Class>().GetListAsync(predicate : x => (x.CourseId.ToString().Equals(course.CourseId.ToString())) && x.Status.Equals("Progressing")));
                if(count == null)
                {
                    course.NumberClassOnGoing = 0;
                }
                else
                {
                    course.NumberClassOnGoing = count.Count();
                }
            }
            return findCourse;
        }

        private async Task<ICollection<Course>> GetDefaultCourse()
        {
            return await _unitOfWork.GetRepository<Course>()
                .GetListAsync(include: x => x
                .Include(x => x.CoursePrerequisites)
                //.Include(x => x.CourseCategory)
                .Include(x => x.Classes)
                .ThenInclude(c => c.Schedules)
                .ThenInclude(s => s.Slot)
                .Include(x => x.SubDescriptionTitles)
                .ThenInclude(sdt => sdt.SubDescriptionContents)
                .Include(x => x.CourseSyllabus)
                .ThenInclude(cs => cs!.Topics.OrderBy(tp => tp.OrderNumber))
                .ThenInclude(tp => tp.Sessions.OrderBy(s => s.NoSession)));
        }

        private async Task<Course[]> GetCoureSubsequents(ICollection<Course> courses)
        {
            var coureSubsequents = new List<Course>();

            foreach (var c in courses)
            {
                var course = await _unitOfWork.GetRepository<Course>()
                    .SingleOrDefaultAsync(predicate: x => x.CoursePrerequisites.Any(cp => cp.PrerequisiteCourseId == c.Id));
                    //include: x => x.Include(x => x.CourseCategory));

                if (course != null)
                {
                    coureSubsequents.Add(course);
                }
            }

            return coureSubsequents.ToArray();
        }

        private async Task<Course[]> GetCoursePrerequesites(ICollection<Course> courses)
        {
            var coursePrerequesites = new List<Course>();

            foreach (var cp in courses.SelectMany(c => c.CoursePrerequisites))
            {
                var course = await _unitOfWork.GetRepository<Course>()
                    .SingleOrDefaultAsync(predicate: c => c.Id == cp.PrerequisiteCourseId);

                if (course != null)
                {
                    coursePrerequesites.Add(course);
                }
            }

            return coursePrerequesites.ToArray();
        }

        public async Task<List<CourseResExtraInfor>> GetCoursesOfStudentByIdAsync(Guid studentId)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == studentId);
            if (student == null)
            {
                throw new BadHttpRequestException($"Id [{studentId}] Học sinh không tồn tại", StatusCodes.Status400BadRequest);
            }
            var courseRegisteredIdList = await _unitOfWork.GetRepository<Course>().GetListAsync(selector: x => x.Id, predicate: x => x.Classes.Any(c => c.StudentClasses.Any(sc => sc.StudentId == studentId)));
            if (courseRegisteredIdList == null)
            {
                return new List<CourseResExtraInfor>();
            }

            var listCourseResExtraInfror = new List<CourseResExtraInfor>();
            foreach (Guid id in courseRegisteredIdList)
            {
                listCourseResExtraInfror.Add(await GetCourseByIdAsync(id));
            }

            return listCourseResExtraInfror;
        }

        public async Task<bool> AddCourseInformation(CreateCourseRequest request)
        {
            if(request != null)
            {
                Course course = new Course
                {
                    AddedDate = DateTime.Now,
                    //CourseCategoryId = Guid.Parse(request.CourseCategoryId),
                    Id = Guid.NewGuid(),
                    Image = request.Img,
                    MaxYearOldsStudent = request.MaxAge,
                    MinYearOldsStudent = request.MinAge,
                    MainDescription = request.MainDescription,
                    Name = request.CourseName,
                    Price = request.Price,
                    UpdateDate = DateTime.Now,
                    Status = "UPCOMING",
                    CourseSyllabusId = Guid.Parse(request.SyllabusId),
                };
                List<SubDescriptionTitle> subDescriptionTitles = new List<SubDescriptionTitle>();
                var listSubDescription = request.SubDescriptions;
                List<SubDescriptionContent> contents = new List<SubDescriptionContent>();
                foreach (var sd in listSubDescription)
                {
                    var newTitle = new SubDescriptionTitle
                    {
                        Title = sd.Title,
                        Id = Guid.NewGuid(),
                        CourseId = course.Id,
                    };
                    var contentList = sd.SubDescriptionContentRequests;
                    foreach (var content in contentList)
                    {
                        var newDescrption = new SubDescriptionContent
                        {
                            SubDescriptionTitleId = newTitle.Id,
                            Content = content.Content,
                            Description = content.Description,
                            Id = Guid.NewGuid(),
                        };
                        contents.Add(newDescrption);
                    }
                    newTitle.SubDescriptionContents = contents;
                    subDescriptionTitles.Add(newTitle);
                }
                List<string> preIds = request.PreRequisiteIds;
                List<CoursePrerequisite> prerequisites = new List<CoursePrerequisite>();
                if(prerequisites != null && prerequisites.Count > 0)
                {
                    foreach (var preId in preIds)
                    {
                        var newPreQ = new CoursePrerequisite
                        {
                            CurrentCourseId = course.Id,
                            Id = Guid.NewGuid(),
                            PrerequisiteCourseId = Guid.Parse(preId),
                        };
                        prerequisites.Add(newPreQ);
                    }
                }
                course.SubDescriptionTitles = subDescriptionTitles;
                var syll = await _unitOfWork.GetRepository<CourseSyllabus>().SingleOrDefaultAsync(predicate :  x => x.Id.ToString().Equals(request.SyllabusId) ,include : x => x.Include(x => x.Topics).ThenInclude(x => x.Sessions));
                var NumOfSess = 0;
                List<Session> sessions = new List<Session>();   
                foreach (var sess in syll.Topics)
                {
                    foreach (var se in sess.Sessions)
                    {
                        sessions.Add(se);
                    }
                }
                course.NumberOfSession = sessions.Count;
                syll.CourseId = course.Id;
                await _unitOfWork.GetRepository<Course>().InsertAsync(course);
                await _unitOfWork.GetRepository<SubDescriptionTitle>().InsertRangeAsync(subDescriptionTitles);
                await _unitOfWork.GetRepository<SubDescriptionContent>().InsertRangeAsync(contents);
                await _unitOfWork.GetRepository<CoursePrerequisite>().InsertRangeAsync(prerequisites);
                 _unitOfWork.GetRepository<CourseSyllabus>().UpdateAsync(syll);
                var isSuccess = await _unitOfWork.CommitAsync() > 0;
                return isSuccess;
            }
            return false;   
        }
    }
}
