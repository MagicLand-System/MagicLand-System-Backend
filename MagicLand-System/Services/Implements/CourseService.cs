using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Courses;
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

        #region thanh_lee code
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
                   .Where(cpf => fc.Syllabus!.SyllabusPrerequisites!.Any(sp => sp.PrerequisiteSyllabusId == cpf.Syllabus!.Id)),
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

            filteredCourses = subject != null
            ? filteredCourses.Where(x => x.SubjectName!.ToLower().Equals(subject.ToLower())).ToList()
            : filteredCourses;

            return filteredCourses;
        }

        public async Task<CourseResExtraInfor> GetCourseByIdAsync(Guid id)
        {
            var course = await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Id == id, include: x => x
            .Include(x => x.Syllabus).ThenInclude(syll => syll!.SyllabusPrerequisites)
            .Include(x => x.Classes)
            .ThenInclude(c => c.Schedules)
            .ThenInclude(s => s.Slot)
            .Include(x => x.SubDescriptionTitles)
            .ThenInclude(sdt => sdt.SubDescriptionContents)
            .Include(x => x.Syllabus)
            .ThenInclude(cs => cs!.Topics!.OrderBy(tp => tp.OrderNumber))
            .ThenInclude(tp => tp.Sessions!.OrderBy(s => s.NoSession)));

            var coursePrerequisites = !course.Any()
                ? throw new BadHttpRequestException($"Id [{id}] Khóa Hoc Không Tồn Tại", StatusCodes.Status400BadRequest)
                : await GetCoursePrerequesites(course);

            var coureSubsequents = await GetCoureSubsequents(course);

            return CourseCustomMapper.fromCourseToCourseResExtraInfor(course.ToList()[0], coursePrerequisites, coureSubsequents);
        }

        public async Task<List<CourseResExtraInfor>> GetCoursesAsync(bool isValid)
        {
            var courses = await GetDefaultCourse();

            if (isValid)
            {
                courses = courses.Where(c => c.Classes.Any() && c.Classes.Any(c => c.Status == ClassStatusEnum.UPCOMING.ToString())).ToList();
            }
            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

            var responses = new List<CourseResExtraInfor>();
            foreach (var course in courses)
            {
                var currentCoursePrerequistites = new List<Course>();
                if (course.Syllabus != null && course.Syllabus!.SyllabusPrerequisites!.Any())
                {
                    foreach (var cp in course.Syllabus!.SyllabusPrerequisites!)
                    {
                        currentCoursePrerequistites = coursePrerequisites.Where(x => x.Syllabus!.Id == cp.PrerequisiteSyllabusId).ToList();
                    }
                }

                responses.Add(CourseCustomMapper.fromCourseToCourseResExtraInfor(course, currentCoursePrerequistites, coureSubsequents));
            }

            return responses;
            //return courses.Select(c => CourseCustomMapper
            //.fromCourseToCourseResExtraInfor(c, coursePrerequisites
            //.Where(cp => c.Syllabus != null && c.Syllabus!.SyllabusPrerequisites!.Any(sp => cp.Syllabus != null && sp.PrerequisiteSyllabusId == cp.Syllabus!.Id)),
            //coureSubsequents)).ToList();
        }


        public async Task<List<CourseResExtraInfor>> SearchCourseByNameOrAddedDateAsync(string keyWord)
        {
            var courses = string.IsNullOrEmpty(keyWord)
            ? await GetDefaultCourse()
            : await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Name!.ToLower().Contains(keyWord.ToLower()), include: x => x
            .Include(x => x.Syllabus).ThenInclude(syll => syll!.SyllabusPrerequisites)
            .Include(x => x.Classes)
            .ThenInclude(c => c.Schedules)
            .ThenInclude(s => s.Slot)
            .Include(x => x.SubDescriptionTitles)
            .ThenInclude(sdt => sdt.SubDescriptionContents)
            .Include(x => x.Syllabus)
            .ThenInclude(cs => cs!.Topics!.OrderBy(tp => tp.OrderNumber))
            .ThenInclude(tp => tp.Sessions!.OrderBy(s => s.NoSession)));

            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

            var currentCoursePrerequistites = new List<Course>();
            foreach (var course in courses)
            {

                if (course.Syllabus != null && course.Syllabus!.SyllabusPrerequisites!.Any())
                {
                    foreach (var cp in course.Syllabus!.SyllabusPrerequisites!)
                    {
                        currentCoursePrerequistites = coursePrerequisites.Where(x => x.Syllabus!.Id == cp.PrerequisiteSyllabusId).ToList();
                    }
                }
            }

            var findCourse = courses.Select(c => CourseCustomMapper
                  .fromCourseToCourseResExtraInfor(c, currentCoursePrerequistites,
                  coureSubsequents)).ToList();

            foreach (var course in findCourse)
            {
                var count = (await _unitOfWork.GetRepository<Class>()
                    .GetListAsync(predicate: x => (x.CourseId.ToString().Equals(course.CourseId.ToString())) && x.Status!.Equals(ClassStatusEnum.PROGRESSING.ToString())));
                if (count == null)
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
            try
            {
                var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(
                    include: x => x.Include(x => x.SubDescriptionTitles).ThenInclude(sdt => sdt.SubDescriptionContents));

                foreach (var course in courses)
                {
                    course.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                    predicate: x => x.Id == course.SyllabusId,
                    include: x => x.Include(x => x.SyllabusPrerequisites!).Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber))
                    .ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)));

                    course.Classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                    predicate: x => x.CourseId == course.Id,
                    include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!));
                }


                return courses;
            }
            catch (Exception e)
            {

                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{e}]", StatusCodes.Status400BadRequest);
            }

        }

        private async Task<Course[]> GetCoureSubsequents(ICollection<Course> courses)
        {
            var coureSubsequents = new List<Course>();

            foreach (var syll in courses.Where(c => c.Syllabus! != null).ToList())
            {
                var course = await _unitOfWork.GetRepository<Syllabus>()
                    .SingleOrDefaultAsync(selector: x => x.Course, predicate: x => x.SyllabusPrerequisites!.Any(sp => sp.PrerequisiteSyllabusId == syll.Id));

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
            var syllabusPrerequisites = courses.Where(c => c.Syllabus! != null && c.Syllabus!.SyllabusPrerequisites! != null).SelectMany(c => c.Syllabus!.SyllabusPrerequisites!).ToList();

            foreach (var cp in syllabusPrerequisites)
            {
                var course = await _unitOfWork.GetRepository<Course>()
                    .SingleOrDefaultAsync(predicate: c => c.Syllabus!.Id == cp.PrerequisiteSyllabusId, include: x => x.Include(x => x.Syllabus).ThenInclude(syll => syll!.SyllabusPrerequisites!));

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
        #endregion
        #region thuong_code
        public async Task<bool> AddCourseInformation(CreateCourseRequest request)
        {
            if (request != null)
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
                    SyllabusId = Guid.Parse(request.SyllabusId),
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
                //List<string> preIds = request.PreRequisiteIds;
                //List<CoursePrerequisite> prerequisites = new List<CoursePrerequisite>();
                //if (prerequisites != null && prerequisites.Count > 0)
                //{
                //    foreach (var preId in preIds)
                //    {
                //        var newPreQ = new CoursePrerequisite
                //        {
                //            CurrentCourseId = course.Id,
                //            Id = Guid.NewGuid(),
                //            PrerequisiteCourseId = Guid.Parse(preId),
                //        };
                //        prerequisites.Add(newPreQ);
                //    }
                //}
                course.SubDescriptionTitles = subDescriptionTitles;
                var syll = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.SyllabusId), include: x => x.Include(x => x.Topics).ThenInclude(x => x.Sessions));
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
                course.SubjectName = syll.SubjectCode;
                syll.CourseId = course.Id;
                await _unitOfWork.GetRepository<Course>().InsertAsync(course);
                await _unitOfWork.GetRepository<SubDescriptionTitle>().InsertRangeAsync(subDescriptionTitles);
                await _unitOfWork.GetRepository<SubDescriptionContent>().InsertRangeAsync(contents);
                //await _unitOfWork.GetRepository<CoursePrerequisite>().InsertRangeAsync(prerequisites);
                _unitOfWork.GetRepository<Syllabus>().UpdateAsync(syll);
                var isSuccess = await _unitOfWork.CommitAsync() > 0;
                return isSuccess;
            }
            return false;
        }

        public async Task<List<SyllabusCategory>> GetCourseCategories()
        {
            var categories = await _unitOfWork.GetRepository<SyllabusCategory>().GetListAsync();
            return categories.ToList();
        }
        #endregion
    }
}
