using AutoMapper;
using Azure;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain.Models.TempEntity.Class;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Courses.Custom;
using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using System.Globalization;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace MagicLand_System.Services.Implements
{
    public class CourseService : BaseService<CourseService>, ICourseService
    {
        public CourseService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<CourseService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        #region thanh_lee code
        public async Task<List<CourseWithScheduleShorten>> FilterCourseAsync(
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

            var filteredCourses = await FilterProgress(minYearsOld, maxYearsOld, minNumberSession, maxNumberSession, minPrice, maxPrice, subject, courses);

            var coursePrerequisitesFilter = await GetCoursePrerequesites(filteredCourses);
            var coureSubsequentsFilter = await GetCoureSubsequents(filteredCourses);
            var responses = filteredCourses.Select(fc => CourseCustomMapper
                   .fromCourseToCourseWithScheduleShorten(fc, default, coursePrerequisitesFilter
                   .Where(cpf => fc.Syllabus!.SyllabusPrerequisites!.Any(sp => sp.PrerequisiteSyllabusId == cpf.Syllabus!.Id)),
                   coureSubsequentsFilter)).ToList();

            foreach (var res in responses)
            {
                res.Price = await GetDynamicPrice(res.CourseId, false);
            }

            return responses;
        }

        private async Task<List<Course>> FilterProgress(int minYearsOld, int maxYearsOld, int? minNumberSession, int? maxNumberSession, double minPrice, double? maxPrice, string? subject, ICollection<Course> courses)
        {
            maxNumberSession ??= int.MaxValue;
            maxPrice ??= double.MaxValue;

            var filteredCourses = minYearsOld > maxYearsOld || minYearsOld < 0 || maxYearsOld < 0
             ? throw new BadHttpRequestException("Độ Tuổi Truy Suất Không Hợp Lệ", StatusCodes.Status400BadRequest)
             : courses.Where(x => x.MinYearOldsStudent >= minYearsOld && x.MaxYearOldsStudent <= maxYearsOld).ToList();


            var fiteredPriceCourse = new List<Course>();
            if (minPrice > maxPrice || minPrice < 0 || maxPrice < 0)
            {
                throw new BadHttpRequestException("Giá Cả Truy Suất Không Hợp Lệ", StatusCodes.Status400BadRequest);
            }
            else
            {
                foreach (var course in filteredCourses)
                {
                    var price = await GetDynamicPrice(course.Id, false);
                    if (price >= minPrice && price <= maxPrice)
                    {
                        fiteredPriceCourse.Add(course);
                    }
                }
            }

            filteredCourses = fiteredPriceCourse.Any() ? fiteredPriceCourse : filteredCourses;

            filteredCourses = filteredCourses.Where(x => x.NumberOfSession >= minNumberSession && x.NumberOfSession <= maxNumberSession).ToList();

            filteredCourses = subject != null
            ? filteredCourses.Where(x => x.SubjectName!.ToLower().Equals(subject.ToLower())).ToList()
            : filteredCourses;

            return filteredCourses;
        }

        public async Task<CourseWithScheduleShorten> GetCourseByIdAsync(Guid id)
        {
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Id == id, include: x => x
            .Include(x => x.SubDescriptionTitles)
            .ThenInclude(sdt => sdt.SubDescriptionContents)
            .Include(x => x.Syllabus!));

            foreach (var course in courses)
            {
                course.Classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.CourseId == course.Id,
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!));

                course.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == course.Id,
                include: x => x.Include(x => x.SyllabusPrerequisites).Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)));
            }

            var coursePrerequisites = !courses.Any()
                ? throw new BadHttpRequestException($"Id [{id}] Khóa Hoc Không Tồn Tại", StatusCodes.Status400BadRequest)
                : await GetCoursePrerequesites(courses);

            var coureSubsequents = await GetCoureSubsequents(courses);

            var response = CourseCustomMapper.fromCourseToCourseWithScheduleShorten(courses.ToList()[0], default, coursePrerequisites, coureSubsequents);
            response.Price = await GetDynamicPrice(response.CourseId, false);
            return response;
        }

        public async Task<List<CourseWithScheduleShorten>> GetCoursesAsync(bool isValid)
        {
            var courses = await GetDefaultCourse();

            if (isValid)
            {
                courses = courses.Where(c => c.Classes.Any() && c.Classes.Any(c => c.Status == ClassStatusEnum.UPCOMING.ToString())).ToList();
            }

            var userId = Guid.Empty;
            if (IsAuthorized())
            {
                var currentRole = GetRoleFromJwt();
                var currentUser = await GetUserFromJwt();
                if (currentRole != RoleEnum.DEVELOPER.ToString())
                {
                    userId = currentRole == RoleEnum.PARENT.ToString() ? currentUser.Id : currentUser.StudentIdAccount!.Value;

                    courses = await FilterCompletedCourses(courses, userId);
                }
            }

            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

            var responses = GenerateCoursesResponse(courses, coursePrerequisites, coureSubsequents);

            await SettingLastResponse(isValid, userId != default ? userId : null, responses);

            return responses;
            //return courses.Select(c => CourseCustomMapper
            //.fromCourseToCourseResExtraInfor(c, coursePrerequisites
            //.Where(cp => c.Syllabus != null && c.Syllabus!.SyllabusPrerequisites!.Any(sp => cp.Syllabus != null && sp.PrerequisiteSyllabusId == cp.Syllabus!.Id)),
            //coureSubsequents)).ToList();
        }

        private async Task SettingLastResponse(bool isValid, Guid? userId, List<CourseWithScheduleShorten> responses)
        {
            if (isValid)
            {
                if (userId == null)
                {
                    responses.ForEach(res => res.IsInCart = null);
                    return;
                }


                var currentUserCart = await _unitOfWork.GetRepository<Cart>().SingleOrDefaultAsync(
                     predicate: x => x.UserId == userId,
                     include: x => x.Include(x => x.CartItems.OrderByDescending(ci => ci.DateCreated)).ThenInclude(cts => cts.StudentInCarts));


                foreach (var response in responses)
                {
                    var item = currentUserCart.CartItems.SingleOrDefault(ci => ci.CourseId == response.CourseId);
                    if (item != null)
                    {
                        response.IsInCart = true;
                        response.CartItemId = item.Id;
                    }
                    else
                    {
                        response.IsInCart = false;
                    }

                }
            }

            foreach (var res in responses)
            {
                res.Price = await GetDynamicPrice(res.CourseId, false);
            }
        }

        private List<CourseWithScheduleShorten> GenerateCoursesResponse(ICollection<Course> courses, Course[] coursePrerequisites, Course[] coureSubsequents)
        {
            var responses = new List<CourseWithScheduleShorten>();
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
                responses.Add(CourseCustomMapper.fromCourseToCourseWithScheduleShorten(course, default, currentCoursePrerequistites, coureSubsequents));
            }

            return responses;
        }

        private async Task<ICollection<Course>> FilterCompletedCourses(ICollection<Course> courses, Guid userId)
        {
            var completeCourses = (await _unitOfWork.GetRepository<StudentClass>().GetListAsync(
                predicate: x => (x.Student!.ParentId == userId || x.Student!.Id == userId) && x.Class!.Status == ClassStatusEnum.COMPLETED.ToString(),
                selector: x => x.Class!.CourseId)).ToList();


            if (completeCourses.Any())
            {
                courses = courses.Where(c => completeCourses.All(cpl => cpl != c.Id)).ToList();
            }

            return courses;
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

        public async Task<List<CourseWithScheduleShorten>> GetCoursesOfStudentByIdAsync(Guid studentId)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == studentId);
            if (student == null)
            {
                throw new BadHttpRequestException($"Id [{studentId}] Học sinh không tồn tại", StatusCodes.Status400BadRequest);
            }
            var courseRegisteredIdList = await _unitOfWork.GetRepository<Course>().GetListAsync(selector: x => x.Id, predicate: x => x.Classes.Any(c => c.StudentClasses.Any(sc => sc.StudentId == studentId)));
            if (courseRegisteredIdList == null)
            {
                return new List<CourseWithScheduleShorten>();
            }

            var courseResponses = new List<CourseWithScheduleShorten>();

            foreach (Guid id in courseRegisteredIdList)
            {
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                    predicate: x => x.Id == id,
                    include: x => x.Include(x => x.SubDescriptionTitles).ThenInclude(sdt => sdt.SubDescriptionContents).Include(x => x.Syllabus!));

                course.Classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.CourseId == course.Id,
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!).Include(x => x.StudentClasses));

                course.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == course.Id,
                include: x => x.Include(x => x.SyllabusPrerequisites).Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)));

                var courses = new List<Course>
                {
                    course
                };

                var coursePrerequisites = await GetCoursePrerequesites(courses);

                var coureSubsequents = await GetCoureSubsequents(courses);

                var response = CourseCustomMapper.fromCourseToCourseWithScheduleShorten(course, studentId, coursePrerequisites, coureSubsequents);
                response.Price = await GetDynamicPrice(id, false);

                courseResponses.Add(response);
            }


            return courseResponses;
        }

        public async Task<List<CourseWithScheduleShorten>> GetCurrentStudentCoursesAsync()
        {
            var currentStudent = await GetUserFromJwt();
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(
                predicate: x => x.Classes.Any(cls => cls.StudentClasses.Any(sc => sc.StudentId == currentStudent.StudentIdAccount)),
                include: x => x.Include(x => x.SubDescriptionTitles).ThenInclude(sdt => sdt.SubDescriptionContents));

            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

            var responses = new List<CourseWithScheduleShorten>();

            foreach (var course in courses)
            {
                course.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Id == course.SyllabusId,
                include: x => x.Include(x => x.SyllabusPrerequisites!).Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber))
                .ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)));

                course.Classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.CourseId == course.Id && x.StudentClasses.Any(sc => sc.StudentId == currentStudent.StudentIdAccount),
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!));

                var currentCoursePrerequistites = new List<Course>();
                if (course.Syllabus != null && course.Syllabus!.SyllabusPrerequisites!.Any())
                {
                    foreach (var cp in course.Syllabus!.SyllabusPrerequisites!)
                    {
                        currentCoursePrerequistites = coursePrerequisites.Where(x => x.Syllabus!.Id == cp.PrerequisiteSyllabusId).ToList();
                    }
                }

                responses.Add(CourseCustomMapper.fromCourseToCourseWithScheduleShorten(course, currentStudent.StudentIdAccount!.Value, currentCoursePrerequistites, coureSubsequents));
            }

            foreach (var res in responses)
            {
                res.Price = await GetDynamicPrice(res.CourseId, false);
            }

            return responses;
        }

        public async Task<List<CourseResExtraInfor>> SearchCourseByNameOrAddedDateAsync(string keyWord)
        {
            var courseCount = (await _unitOfWork.GetRepository<Course>().GetListAsync()).Count;
            var courses = string.IsNullOrEmpty(keyWord)
            ? await GetDefaultCourse()
            : await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Name!.ToLower().Contains(keyWord.ToLower()), include: x => x
             .Include(x => x.Syllabus)
            .Include(x => x.SubDescriptionTitles)
            .ThenInclude(sdt => sdt.SubDescriptionContents));

            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

            var currentCoursePrerequistites = new List<Course>();
            foreach (var course in courses)
            {
                course.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == course.Id,
                include: x => x.Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(s => s.NoSession)).Include(x => x.SyllabusPrerequisites)!);


                course.Classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.CourseId == course.Id,
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)!);

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
                var count = (await _unitOfWork.GetRepository<Class>().GetListAsync(
                    predicate: x => (x.CourseId.ToString().Equals(course.CourseId.ToString())) && (x.Status!.Equals(ClassStatusEnum.PROGRESSING.ToString()) || x.Status.Equals("UPCOMING"))));

                if (count == null)
                {
                    course.NumberClassOnGoing = 0;
                }
                else
                {
                    course.NumberClassOnGoing = count.Count();
                }
                course.NumberOfCourses = courseCount;
                var coursex = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(
                    course.CourseDetail!.Id.ToString()),
                    include: x => x.Include(x => x.Syllabus).ThenInclude(x => x!.SyllabusCategory)!);

                var name = "undefined";
                if (coursex.Syllabus != null)
                {
                    name = coursex.Syllabus.SyllabusCategory!.Name;
                }
                course.CourseDetail!.Subject = name;
            }

            findCourse = findCourse.OrderByDescending(x => x.UpdateDate).ToList();
            return findCourse;
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
                    //Price = request.Price,
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
                var syll = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.SyllabusId), include: x => x.Include(x => x.Topics).ThenInclude(x => x.Sessions).Include(x => x.SyllabusCategory));
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
                //course.SubjectName = syll.SubjectCode;
                course.SubjectName = syll.SyllabusCategory.Name;
                //syll.CourseId = course.Id;
                await _unitOfWork.GetRepository<Course>().InsertAsync(course);
                await _unitOfWork.GetRepository<SubDescriptionTitle>().InsertRangeAsync(subDescriptionTitles);
                await _unitOfWork.GetRepository<SubDescriptionContent>().InsertRangeAsync(contents);
                //await _unitOfWork.GetRepository<CoursePrerequisite>().InsertRangeAsync(prerequisites);
                _unitOfWork.GetRepository<Syllabus>().UpdateAsync(syll);
                CoursePrice coursePrice = new CoursePrice
                {
                    CourseId = course.Id,
                    //EffectiveDate = DateTime.UtcNow,
                    Price = request.Price,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.ParseExact("2040-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                };

                await _unitOfWork.GetRepository<CoursePrice>().InsertAsync(coursePrice);
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

        public async Task<StaffCourseResponse> GetStaffCourseByCourseId(string courseid)
        {
            var courseFound = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseid), include: x => x.Include(x => x.SubDescriptionTitles));
            if (courseFound == null)
            {
                throw new BadHttpRequestException("Không tìm thấy khóa thích hợp", StatusCodes.Status400BadRequest);
            }
            var courseSyllabus = courseFound.SyllabusId;
            var subjectname = "undefined";
            var syllabusCode = "undefined";
            if (courseSyllabus != null)
            {
                subjectname = (await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseSyllabus.ToString()), include: x => x.Include(x => x.SyllabusCategory))).SyllabusCategory.Name;
                syllabusCode = (await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseSyllabus.ToString()))).SubjectCode;
            }
            var titles = courseFound.SubDescriptionTitles;
            List<SubDescriptionTitleResponse> desResponse = new List<SubDescriptionTitleResponse>();
            foreach (var title in titles)
            {
                List<SubDescriptionContentResponse> req = new List<SubDescriptionContentResponse>();
                var content = new SubDescriptionTitleResponse
                {
                    Title = title.Title,
                };
                var sc = (await _unitOfWork.GetRepository<SubDescriptionContent>().GetListAsync(predicate: x => x.SubDescriptionTitleId.ToString().Equals(title.Id.ToString()))).ToList();
                foreach (var s in sc)
                {
                    req.Add(new SubDescriptionContentResponse
                    {
                        Content = s.Content,
                        Description = s.Description,
                    });
                }
                content.Contents = req;
                desResponse.Add(content);

            }
            var priceList = await _unitOfWork.GetRepository<CoursePrice>().GetListAsync(predicate: x => x.CourseId.ToString().Equals(courseid));
            //var priceArray = priceList.OrderByDescending(x => x.EffectiveDate).ToArray();
            int ongoing = 0;
            var count = (await _unitOfWork.GetRepository<Class>()
                    .GetListAsync(predicate: x => (x.CourseId.ToString().Equals(courseFound.Id.ToString())) && (x.Status!.Equals(ClassStatusEnum.PROGRESSING.ToString()) || x.Status.Equals("UPCOMING"))));
            if (count == null)
            {
                ongoing = 0;
            }
            else
            {
                ongoing = count.Count();
            }
            StaffCourseResponse staffCourseResponse = new StaffCourseResponse
            {
                AddedDate = courseFound.AddedDate,
                Id = courseFound.Id,
                Image = courseFound.Image,
                MainDescription = courseFound.MainDescription,
                MaxYearOldsStudent = courseFound.MaxYearOldsStudent,
                MinYearOldsStudent = courseFound.MinYearOldsStudent,
                Name = courseFound.Name,
                NumberOfSession = courseFound.NumberOfSession,
                Price = await GetDynamicPrice(courseFound.Id, false),
                Status = courseFound.Status,
                SubjectName = subjectname,
                SyllabusId = courseFound.SyllabusId,
                UpdateDate = courseFound.UpdateDate,
                SubDescriptionTitles = desResponse,
                NumberOfClassOnGoing = ongoing,
                SyllabusCode = syllabusCode,
            };

            return staffCourseResponse;
        }



        public async Task<bool> GenerateCoursePrice(CoursePriceRequest request)
        {
            if (request != null)
            {
                if (request.StartTime < DateTime.Now.AddMinutes(-120))
                {
                    throw new BadHttpRequestException("ngày hiệu lực không thể ở trong quá khứ", StatusCodes.Status400BadRequest);
                }
                if (request.EndTime < DateTime.Now.AddMinutes(-120))
                {
                    throw new BadHttpRequestException("ngày hiệu lực không thể ở trong quá khứ", StatusCodes.Status400BadRequest);
                }
                if (request.Price < 0)
                {
                    throw new BadHttpRequestException("giá phải lớn hơn 0", StatusCodes.Status400BadRequest);
                }
                var price = new CoursePrice
                {
                    Price = request.Price,
                    CourseId = request.CourseId,
                    //EffectiveDate = request.EffectiveDate,
                    StartDate = request.StartTime.AddHours(7),
                    EndDate = request.EndTime.AddHours(7),
                    Id = Guid.NewGuid(),
                };
                await _unitOfWork.GetRepository<CoursePrice>().InsertAsync(price);
                bool isSuccess = await _unitOfWork.CommitAsync() > 0;
                return isSuccess;
            }
            return false;
        }

        public async Task<List<CoursePrice>> GetCoursePrices(string courseId)
        {
            var prices = await _unitOfWork.GetRepository<CoursePrice>().GetListAsync(predicate: x => x.CourseId.ToString().Equals(courseId));
            return prices.ToList();
        }

        public async Task<List<StaffCourseResponse>> GetCourseResponse(List<string>? categoryIds, string? searchString, int? minAge, int? MaxAge)
        {
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(
                predicate: x => x.Classes.Any(cls => cls.Status == ClassStatusEnum.UPCOMING.ToString()),
                include: x => x.Include(x => x.SubDescriptionTitles).ThenInclude(sd => sd.SubDescriptionContents).Include(x => x.Classes));

            if (courses == null || !courses.Any())
            {
                throw new BadHttpRequestException("Không tìm thấy khóa thích hợp", StatusCodes.Status400BadRequest);
            }

            var result = new List<StaffCourseResponse>();
            await GetCourseStaffResult(courses, result);

            result = GetFinalCourseStaffResult(categoryIds, searchString, minAge, MaxAge, result);

            return result;
        }

        private async Task GetCourseStaffResult(ICollection<Course> courses, List<StaffCourseResponse> result)
        {
            foreach (var course in courses)
            {
                var subject = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                    selector: x => x.SyllabusCategory!,
                    predicate: x => x.Id == course.SyllabusId);

                var titles = course.SubDescriptionTitles;
                var sdtr = new List<SubDescriptionTitleResponse>();
                foreach (var title in titles)
                {
                    var sdcr = new List<SubDescriptionContentResponse>();
                    foreach (var content in title.SubDescriptionContents)
                    {
                        sdcr.Add(new SubDescriptionContentResponse
                        {
                            Content = content.Content,
                            Description = content.Description,
                        });
                    }

                    sdtr.Add(new SubDescriptionTitleResponse
                    {
                        Title = title.Title,
                        Contents = sdcr,
                    });

                }
                //var priceList = await _unitOfWork.GetRepository<CoursePrice>().GetListAsync(predicate: x => x.CourseId == course.Id);

                var ongoing = (await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.CourseId == course.Id && (x.Status!.Equals(ClassStatusEnum.PROGRESSING.ToString()) || x.Status.Equals(ClassStatusEnum.UPCOMING.ToString())))).Count;

                var classList = course.Classes.ToList();
                if (classList == null || classList.Count == 0)
                {
                    continue;
                }

                var earliestDate = course.Classes.Where(x => x.Status!.Equals(ClassStatusEnum.UPCOMING.ToString()) && x.StartDate >= DateTime.Now).OrderBy(x => x.StartDate);

                var staffCourseResponse = new StaffCourseResponse
                {
                    AddedDate = course.AddedDate,
                    Id = course.Id,
                    Image = course.Image,
                    MainDescription = course.MainDescription,
                    MaxYearOldsStudent = course.MaxYearOldsStudent,
                    MinYearOldsStudent = course.MinYearOldsStudent,
                    Name = course.Name,
                    NumberOfSession = course.NumberOfSession,
                    Price = await GetDynamicPrice(course.Id, false),
                    Status = course.Status,
                    SubjectName = subject.Name,
                    SyllabusId = course.SyllabusId,
                    UpdateDate = course.UpdateDate,
                    SubDescriptionTitles = sdtr,
                    NumberOfClassOnGoing = ongoing,
                    EarliestClassTime = earliestDate.Any() ? earliestDate.First().StartDate : course.Classes.OrderBy(x => x.StartDate).First().StartDate,
                    CategoryId = subject.Id.ToString(),
                };

                result.Add(staffCourseResponse);
            }
        }

        private List<StaffCourseResponse> GetFinalCourseStaffResult(List<string>? categoryIds, string? searchString, int? minAge, int? MaxAge, List<StaffCourseResponse> result)
        {
            if (result.Count > 0)
            {
                List<StaffCourseResponse> result1 = new List<StaffCourseResponse>(), result2 = new List<StaffCourseResponse>(), result3 = new List<StaffCourseResponse>(), result4 = new List<StaffCourseResponse>();

                if (categoryIds != null && categoryIds.Count > 0)
                {
                    result1 = result.Where(x => categoryIds.Contains(x.CategoryId.ToString())).ToList();
                }
                if (minAge != null)
                {
                    result2 = result.Where(x => x.MinYearOldsStudent >= minAge.Value).ToList();
                }
                if (MaxAge != null)
                {
                    result3 = result.Where(x => (x.MaxYearOldsStudent >= MaxAge.Value && x.MinYearOldsStudent <= MaxAge.Value) || (x.MaxYearOldsStudent >= minAge.Value && x.MinYearOldsStudent <= minAge.Value)).ToList();
                }
                if (searchString != null)
                {
                    result4 = result.Where(x => x.Name.ToLower().Trim().Contains(searchString.ToLower().Trim())).ToList();
                }
                if (result1.Count > 0 || result2.Count > 0 || result3.Count > 0 || result4.Count > 0)
                {
                    var resultT = result1.UnionBy(result3, x => x.Id).UnionBy(result4, x => x.Id);
                    result = resultT.ToList();
                }
            }

            return result;
        }

        public async Task<List<MyClassResponse>> GetClassesOfCourse(string courseId, List<string>? dateOfWeeks, string? Method, List<string>? slotId)
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.CourseId.ToString().Equals(courseId) && x.Status.Equals("UPCOMING"), include: x => x.Include(x => x.Schedules));
            List<MyClassResponse> result = new List<MyClassResponse>();
            var slots = await _unitOfWork.GetRepository<Slot>().GetListAsync();
            foreach (var c in classes)
            {
                var schedulex = (await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId == c.Id)).FirstOrDefault();
                if (schedulex == null) { continue; }
                var room = (await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id == schedulex.RoomId));
                if (room == null) { continue; }
                var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.LecturerId.ToString()));
                if (lecturer == null) { continue; }
                var students = (await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id)).Count;
                RoomResponse roomResponse = new RoomResponse
                {
                    Floor = room.Floor.Value,
                    Capacity = room.Capacity,
                    RoomId = room.Id,
                    Name = room.Name,
                    Status = room.Status,
                    LinkUrl = room.LinkURL,

                };
                LecturerResponse lecturerResponse = new LecturerResponse
                {
                    AvatarImage = lecturer.AvatarImage,
                    DateOfBirth = lecturer.DateOfBirth,
                    Email = lecturer.Email,
                    FullName = lecturer.FullName,
                    Gender = lecturer.Gender,
                    LectureId = lecturer.Id,
                    Phone = lecturer.Phone,
                };
                List<DailySchedule> schedules = new List<DailySchedule>();
                var DaysOfWeek = c.Schedules.Select(c => new { c.DayOfWeek, c.SlotId }).Distinct().ToList();
                foreach (var day in DaysOfWeek)
                {
                    var slot = slots.Where(x => x.Id.ToString().ToLower().Equals(day.SlotId.ToString().ToLower())).FirstOrDefault();
                    if (day.DayOfWeek == 1)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Sunday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                            SlotId = slot.Id,
                            DateOfWeekNumber = day.DayOfWeek,
                        });
                    }
                    if (day.DayOfWeek == 2)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Monday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                            SlotId = slot.Id,
                            DateOfWeekNumber = day.DayOfWeek,
                        });
                    }
                    if (day.DayOfWeek == 4)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Tuesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                            SlotId = slot.Id,
                            DateOfWeekNumber = day.DayOfWeek,
                        });
                    }
                    if (day.DayOfWeek == 8)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Wednesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                            SlotId = slot.Id,
                            DateOfWeekNumber = day.DayOfWeek,
                        });
                    }
                    if (day.DayOfWeek == 16)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Thursday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                            SlotId = slot.Id,
                            DateOfWeekNumber = day.DayOfWeek,

                        });
                    }
                    if (day.DayOfWeek == 32)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Friday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                            SlotId = slot.Id,
                            DateOfWeekNumber = day.DayOfWeek,
                        });
                    }
                    if (day.DayOfWeek == 64)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Saturday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                            SlotId = slot.Id,
                            DateOfWeekNumber = day.DayOfWeek,
                        });
                    }
                }
                schedules = schedules.OrderBy(x => x.DateOfWeekNumber).ToList();
                Course course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(c.CourseId.ToString()), include: x => x.Include(x => x.Syllabus).ThenInclude(x => x.SyllabusCategory));
                var studentList = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == c.Id);
                MyClassResponse myClassResponse = new MyClassResponse
                {
                    ClassId = c.Id,
                    LimitNumberStudent = c.LimitNumberStudent,
                    ClassCode = c.ClassCode,
                    LecturerName = lecturer.FullName,
                    CoursePrice = await GetDynamicPrice(course.Id, false),
                    EndDate = c.EndDate,
                    CourseId = c.CourseId,
                    Image = c.Image,
                    LeastNumberStudent = c.LeastNumberStudent,
                    Method = c.Method,
                    StartDate = c.StartDate,
                    Status = c.Status,
                    Video = c.Video,
                    NumberStudentRegistered = studentList.Count,
                    Schedules = schedules,
                    CourseName = course.Name,
                    LecturerResponse = lecturerResponse,
                    RoomResponse = roomResponse,
                    CreatedDate = c.AddedDate.Value,
                    NumberOfStudentsRegister = students,
                };
                var syllabusCode = "undefined";
                var syllabusName = "undefined";
                var syllabusType = "undefined";
                if (course.Syllabus != null)
                {
                    syllabusCode = course.Syllabus.SubjectCode;
                    syllabusName = course.Syllabus.Name;
                    syllabusType = course.Syllabus.SyllabusCategory.Name;
                }
                CustomCourseResponse customCourseResponse = new CustomCourseResponse
                {
                    Image = course.Image,
                    MainDescription = course.MainDescription,
                    MaxYearOldsStudent = course.MaxYearOldsStudent,
                    MinYearOldsStudent = course.MinYearOldsStudent,
                    Name = course.Name,
                    Price = await GetDynamicPrice(course.Id, false),
                    SyllabusCode = syllabusCode,
                    SyllabusName = syllabusName,
                    SyllabusType = syllabusType,
                    Status = course.Status
                };
                myClassResponse.CourseResponse = customCourseResponse;
                result.Add(myClassResponse);
            }
            if (result.Count == 0)
            {
                return result;
            }
            List<MyClassResponse> finalResponse = new List<MyClassResponse>();
            List<MyClassResponse> finalResponse2 = new List<MyClassResponse>();
            List<MyClassResponse> finalResponse3 = new List<MyClassResponse>();
            if (result.Count > 0)
            {
                if (dateOfWeeks != null && dateOfWeeks.Count > 0)
                {
                    foreach (var r in result)
                    {
                        var sch = r.Schedules;
                        foreach (var d in dateOfWeeks)
                        {
                            var schList = sch.Select(x => x.DayOfWeek).ToList();
                            var isExist = schList.Any(x => x.ToLower().Equals(d.ToLower()));
                            if (isExist)
                            {
                                finalResponse.Add(r);
                                break;
                            }
                        }
                    }
                }
                if (Method != null)
                {
                    finalResponse2 = result.Where(x => x.Method.ToLower().Contains(Method.ToLower())).ToList();
                }
                if (slotId != null && slotId.Count > 0)
                {
                    foreach (var r in result)
                    {
                        var sch = r.Schedules;
                        foreach (var d in slotId)
                        {
                            var schList = sch.Select(x => x.SlotId).ToList();
                            var isExist = schList.Any(x => x.ToString().ToLower().Equals(d.ToLower()));
                            if (isExist)
                            {
                                finalResponse3.Add(r);
                                break;
                            }
                        }
                    }
                }
                if (finalResponse.Count > 0 || finalResponse2.Count > 0 || finalResponse3.Count > 0)
                {
                    var rs = finalResponse.UnionBy(finalResponse2, x => x.ClassId).UnionBy(finalResponse3, x => x.ClassId).ToList();
                    return rs.OrderByDescending(x => x.NumberOfStudentsRegister).ToList();
                }

            }
            if ((dateOfWeeks == null || dateOfWeeks.Count == 0) && (Method == null) && (slotId == null || slotId.Count == 0))
            {
                return result.OrderByDescending(x => x.NumberOfStudentsRegister).ToList();
            }
            return new List<MyClassResponse>();
        }
        public async Task<bool> UpdateCourse(string id, UpdateCourseRequest request)
        {
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id), include: x => x.Include(x => x.CoursePrices));
            var courseprice = course.CoursePrices.SingleOrDefault(predicate: x => x.EndDate >= DateTime.Now.AddYears(13));
            if (course == null)
            {
                return false;
            }
            if (request.Price != null)
            {
                courseprice.Price = request.Price.Value;
            }
            if (request.Img != null)
            {
                course.Image = request.Img;
            }
            if (request.CourseName != null)
            {
                course.Name = request.CourseName;
            }
            if (request.MaxAge != null)
            {
                course.MaxYearOldsStudent = request.MaxAge.Value;
            }
            if (request.MinAge != null)
            {
                course.MinYearOldsStudent = request.MinAge.Value;
            }
            if (request.MainDescription != null)
            {
                course.MainDescription = request.MainDescription;
            }
            if (request.SubDescriptions != null)
            {
                var titles = await _unitOfWork.GetRepository<SubDescriptionTitle>().GetListAsync(predicate: x => x.CourseId == course.Id);
                var courseDes = new List<SubDescriptionContent>();
                foreach (var title in titles)
                {
                    courseDes.AddRange(await _unitOfWork.GetRepository<SubDescriptionContent>().GetListAsync(predicate: x => x.SubDescriptionTitleId == title.Id));
                }
                _unitOfWork.GetRepository<SubDescriptionContent>().DeleteRangeAsync(courseDes);
                _unitOfWork.GetRepository<SubDescriptionTitle>().DeleteRangeAsync(titles);
                await _unitOfWork.CommitAsync();
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
                    course.SubDescriptionTitles = subDescriptionTitles;
                    await _unitOfWork.GetRepository<SubDescriptionTitle>().InsertRangeAsync(subDescriptionTitles);
                    await _unitOfWork.GetRepository<SubDescriptionContent>().InsertRangeAsync(contents);
                }
            }
            _unitOfWork.GetRepository<Course>().UpdateAsync(course);
            _unitOfWork.GetRepository<CoursePrice>().UpdateAsync(courseprice);
            bool isSuccess = await _unitOfWork.CommitAsync() > 0;
            return isSuccess;
        }

        public async Task<List<CourseWithScheduleShorten>> GetCourseByStaff()
        {
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(
                include: x => x.Include(x => x.SubDescriptionTitles).ThenInclude(sdt => sdt.SubDescriptionContents));

            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

            var responses = new List<CourseWithScheduleShorten>();

            foreach (var course in courses)
            {
                course.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Id == course.SyllabusId,
                include: x => x.Include(x => x.SyllabusPrerequisites!).Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber))
                .ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)));

                course.Classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!));

                var currentCoursePrerequistites = new List<Course>();
                if (course.Syllabus != null && course.Syllabus!.SyllabusPrerequisites!.Any())
                {
                    foreach (var cp in course.Syllabus!.SyllabusPrerequisites!)
                    {
                        currentCoursePrerequistites = coursePrerequisites.Where(x => x.Syllabus!.Id == cp.PrerequisiteSyllabusId).ToList();
                    }
                }

                responses.Add(CourseCustomMapper.fromCourseToCourseWithScheduleShorten(course, default, currentCoursePrerequistites, coureSubsequents));
            }

            foreach (var res in responses)
            {
                res.Price = await GetDynamicPrice(res.CourseId, false);
            }

            return responses;
        }

        public async Task<CourseSearchResponse> GetCourseSearch(string keyword)
        {
            var courseCount = (await _unitOfWork.GetRepository<Course>().GetListAsync()).Count;
            var courses = string.IsNullOrEmpty(keyword)
            ? await GetDefaultCourse()
            : await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Name!.ToLower().Contains(keyword.ToLower()), include: x => x
             .Include(x => x.Syllabus)
            .Include(x => x.SubDescriptionTitles)
            .ThenInclude(sdt => sdt.SubDescriptionContents));

            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

            var currentCoursePrerequistites = new List<Course>();
            foreach (var course in courses)
            {
                course.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == course.Id,
                include: x => x.Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(s => s.NoSession)).Include(x => x.SyllabusPrerequisites)!);


                course.Classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.CourseId == course.Id,
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)!);

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
                var count = (await _unitOfWork.GetRepository<Class>().GetListAsync(
                    predicate: x => (x.CourseId.ToString().Equals(course.CourseId.ToString())) && (x.Status!.Equals(ClassStatusEnum.PROGRESSING.ToString()) || x.Status.Equals("UPCOMING"))));

                if (count == null)
                {
                    course.NumberClassOnGoing = 0;
                }
                else
                {
                    course.NumberClassOnGoing = count.Count();
                }
                course.NumberOfCourses = courseCount;
                var coursex = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(
                    course.CourseDetail!.Id.ToString()),
                    include: x => x.Include(x => x.Syllabus).ThenInclude(x => x!.SyllabusCategory)!);

                var name = "undefined";
                if (coursex.Syllabus != null)
                {
                    name = coursex.Syllabus.SyllabusCategory!.Name;
                }
                course.CourseDetail!.Subject = name;
            }

            findCourse = findCourse.OrderByDescending(x => x.UpdateDate).ToList();
            return new CourseSearchResponse
            {
                Courses = findCourse,
                NumberOfCourse = courseCount,
            };
        }

        public async Task<bool> RegisterSavedCourse(string studentId,string courseId, string classId)
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.CourseId.ToString().Equals(courseId),selector : x => x.Id);
            var studentClass = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(predicate: x => x.StudentId.ToString().Equals(studentId) && classes.Any(p => p == x.ClassId));
            if(studentClass == null) 
            {
                throw new BadHttpRequestException("Bạn chưa đăng ký lớp học nào  của kháo");
            }
            if (!studentClass.Status.Equals("Saved"))
            {
                throw new BadHttpRequestException("Bạn chưa bảo lưu khóa học");
            }
            var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId == studentClass.ClassId,selector : x => x.Id);
            var attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId) && schedules.Any(p => p == x.ScheduleId));
            _unitOfWork.GetRepository<Attendance>().DeleteRangeAsync(attendances);
            _unitOfWork.GetRepository<StudentClass>().DeleteAsync(studentClass);
            await _unitOfWork.CommitAsync();
            var newStudentClass = new StudentClass
            {
                AddedTime = DateTime.Now,
                CanChangeClass = true,
                ClassId = Guid.Parse(classId),
                StudentId = Guid.Parse(studentId),
                Status = "ChangeFromAnotherClass",
                SavedTime = DateTime.Now,
                Id = Guid.NewGuid(),
            };
            await _unitOfWork.GetRepository<StudentClass>().InsertAsync(newStudentClass);
            await _unitOfWork.CommitAsync();
            var newClass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId));
            var newSchedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.ClassId.ToString().Equals(classId));
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate : x => x.Id == studentClass.StudentId,include : x => x.Include(x => x.User));
            List<Attendance> attendances1 = new List<Attendance>();
            foreach( var schedule in newSchedule)
            {
                var attendance = new Attendance
                {
                    Id = Guid.NewGuid(),
                    IsPresent = null,
                    IsPublic = true,
                    ScheduleId = schedule.Id,
                    StudentId = studentClass.StudentId,
                    IsValid = true,
                };
                attendances1.Add(attendance);
             
            }
            await _unitOfWork.GetRepository<Attendance>().InsertRangeAsync(attendances1);
            await _unitOfWork.CommitAsync();
            var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                        {
                          ($"{AttachValueEnum.ClassId}", $"{newClass.Id}"),
                          ($"{AttachValueEnum.StudentId}", $"{studentClass.StudentId}"),
                        });
            var listItemIdentify = new List<string>
                {
                          StringHelper.TrimStringAndNoSpace(student.User.Id.ToString()),
                          StringHelper.TrimStringAndNoSpace("Chuyển lớp sau khi bảo lưu"),
                          StringHelper.TrimStringAndNoSpace($"Bạn đã được chuyển vào lớp {newClass.ClassCode}"),
                          StringHelper.TrimStringAndNoSpace(newClass.Id.ToString()),
                          StringHelper.TrimStringAndNoSpace(actionData),
                };

            var newNotification = new Notification
            {
                Id = Guid.NewGuid(),
                Body = $"Bạn đã được chuyển vào lớp {newClass.ClassCode}",
                CreatedAt = DateTime.Now,
                Image = newClass.Image!,
                IsRead = false,
                Title = "Chuyển sau khi bảo lưu",
                Priority =  NotificationPriorityEnum.IMPORTANCE.ToString(),
                ActionData = actionData,
                UserId = student.User.Id,
                Identify = StringHelper.ComputeSHA256Hash(string.Join("", listItemIdentify)),
            };
            await _unitOfWork.GetRepository<Notification>().InsertAsync(newNotification);
            return await _unitOfWork.CommitAsync() > 0;
        }
        #endregion
    }
}
