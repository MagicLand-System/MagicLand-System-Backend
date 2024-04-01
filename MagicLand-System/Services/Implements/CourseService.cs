using AutoMapper;
using Azure;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain.Models.TempEntity.Class;
using MagicLand_System.Enums;
using MagicLand_System.Mappers.Custom;
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
using System.Globalization;

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
                   .fromCourseToCourseWithScheduleShorten(fc, coursePrerequisitesFilter
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
                predicate: x => x.CourseId == course.Id,
                include: x => x.Include(x => x.SyllabusPrerequisites).Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)));
            }

            var coursePrerequisites = !courses.Any()
                ? throw new BadHttpRequestException($"Id [{id}] Khóa Hoc Không Tồn Tại", StatusCodes.Status400BadRequest)
                : await GetCoursePrerequesites(courses);

            var coureSubsequents = await GetCoureSubsequents(courses);

            var response = CourseCustomMapper.fromCourseToCourseWithScheduleShorten(courses.ToList()[0], coursePrerequisites, coureSubsequents);
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
            var coursePrerequisites = await GetCoursePrerequesites(courses);
            var coureSubsequents = await GetCoureSubsequents(courses);

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
                responses.Add(CourseCustomMapper.fromCourseToCourseWithScheduleShorten(course, currentCoursePrerequistites, coureSubsequents));
            }


            if (isValid)
            {
                var currentUserCart = await _unitOfWork.GetRepository<Cart>().SingleOrDefaultAsync(
                     predicate: x => x.UserId == GetUserIdFromJwt(),
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

            return responses;
            //return courses.Select(c => CourseCustomMapper
            //.fromCourseToCourseResExtraInfor(c, coursePrerequisites
            //.Where(cp => c.Syllabus != null && c.Syllabus!.SyllabusPrerequisites!.Any(sp => cp.Syllabus != null && sp.PrerequisiteSyllabusId == cp.Syllabus!.Id)),
            //coureSubsequents)).ToList();
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

        public async Task<List<CourseResponseCustom>> GetCoursesOfStudentByIdAsync(Guid studentId)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == studentId);
            if (student == null)
            {
                throw new BadHttpRequestException($"Id [{studentId}] Học sinh không tồn tại", StatusCodes.Status400BadRequest);
            }
            var courseRegisteredIdList = await _unitOfWork.GetRepository<Course>().GetListAsync(selector: x => x.Id, predicate: x => x.Classes.Any(c => c.StudentClasses.Any(sc => sc.StudentId == studentId)));
            if (courseRegisteredIdList == null)
            {
                return new List<CourseResponseCustom>();
            }

            var listCourseResExtraInfror = new List<CourseResponseCustom>();

            foreach (Guid id in courseRegisteredIdList)
            {
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                    predicate: x => x.Id == id,
                    include: x => x.Include(x => x.SubDescriptionTitles).ThenInclude(sdt => sdt.SubDescriptionContents).Include(x => x.Syllabus!));

                course.Classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.CourseId == course.Id,
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!));

                course.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.CourseId == course.Id,
                include: x => x.Include(x => x.SyllabusPrerequisites).Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)));

                var courses = new List<Course>
                {
                    course
                };

                var coursePrerequisites = await GetCoursePrerequesites(courses);

                var coureSubsequents = await GetCoureSubsequents(courses);

                var response = CourseCustomMapper.fromCourseToCourseReponseCustom(courses.ToList()[0], coursePrerequisites, coureSubsequents);
                response.Price = await GetDynamicPrice(id, false);

                listCourseResExtraInfror.Add(response);
            }


            return listCourseResExtraInfror;
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

                responses.Add(CourseCustomMapper.fromCourseToCourseWithScheduleShorten(course, currentCoursePrerequistites, coureSubsequents));
            }

            foreach (var res in responses)
            {
                res.Price = await GetDynamicPrice(res.CourseId, false);
            }

            return responses;
        }

        public async Task<List<CourseResExtraInfor>> SearchCourseByNameOrAddedDateAsync(string keyWord)
        {
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
                predicate: x => x.CourseId == course.Id,
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
                syll.CourseId = course.Id;
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
            if (courseSyllabus != null)
            {
                subjectname = (await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseSyllabus.ToString()), include: x => x.Include(x => x.SyllabusCategory))).SyllabusCategory.Name;
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
            };

            return staffCourseResponse;
        }



        public async Task<bool> GenerateCoursePrice(CoursePriceRequest request)
        {
            if(request == null)
            {
                if (request.EffectiveDate < DateTime.Now.AddMinutes(-5))
                {
                    throw new BadHttpRequestException("ngày hiệu lực không thể ở trong quá khứ",StatusCodes.Status400BadRequest);
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
            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(include: x => x.Include(x => x.Classes));
            List<Course> courseList = new List<Course>();
            foreach (var course in courses)
            {
                var classList = course.Classes.ToList();
                var isExist = classList.Any(x => x.Status.Equals("UPCOMING"));
                if(isExist)
                {
                    courseList.Add(course);
                }
            }
            List<StaffCourseResponse> result = new List<StaffCourseResponse>();
            foreach(var course in courseList)
            {
                var courseFound = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(course.Id.ToString()), include: x => x.Include(x => x.SubDescriptionTitles).Include(x => x.Classes));
                if (courseFound == null)
                {
                    throw new BadHttpRequestException("Không tìm thấy khóa thích hợp", StatusCodes.Status400BadRequest);
                }
                var courseSyllabus = courseFound.SyllabusId;
                var subjectname = "undefined";
                string categoryId = "";
                if (courseSyllabus != null)
                {
                    subjectname = (await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseSyllabus.ToString()), include: x => x.Include(x => x.SyllabusCategory))).SyllabusCategory.Name;
                    categoryId = (await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseSyllabus.ToString()), include: x => x.Include(x => x.SyllabusCategory))).SyllabusCategory.Id.ToString();    
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
                var priceList = await _unitOfWork.GetRepository<CoursePrice>().GetListAsync(predicate: x => x.CourseId.ToString().Equals(course.Id.ToString()));
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
                var classList = course.Classes.ToList();
                var earliestDate = (course.Classes.Where(x => x.Status.Equals("UPCOMING") && x.StartDate >= DateTime.Now).OrderBy(x => x.StartDate).ToArray())[0].StartDate;
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
                    Price = await GetDynamicPrice(course.Id,false),
                    Status = courseFound.Status,
                    SubjectName = subjectname,
                    SyllabusId = courseFound.SyllabusId,
                    UpdateDate = courseFound.UpdateDate,
                    SubDescriptionTitles = desResponse,
                    NumberOfClassOnGoing = ongoing,
                    EarliestClassTime = earliestDate,
                    CategoryId = categoryId,
                };
                result.Add(staffCourseResponse);
            }
            if(result.Count > 0)
            {
                if(categoryIds != null && categoryIds.Count > 0) 
                {
                    result = result.Where(x => categoryIds.Contains(x.CategoryId.ToString())).ToList(); 
                }
                if(minAge != null)
                {
                    result = result.Where(x => x.MinYearOldsStudent >= minAge.Value).ToList();
                }
                if(MaxAge != null)
                {
                    result = result.Where(x => x.MaxYearOldsStudent <= MaxAge.Value).ToList();
                }
                if(searchString != null)
                {
                    result = result.Where(x => x.Name.ToLower().Trim().Contains(searchString.ToLower().Trim())).ToList();
                }
            }
            return result;
        }

        public async Task<List<MyClassResponse>> GetClassesOfCourse(string courseId, List<string>? dateOfWeeks)
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate : x => x.CourseId.ToString().Equals(courseId) && x.Status.Equals("UPCOMING"),include: x => x.Include(x => x.Schedules));
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
                        });
                    }
                    if (day.DayOfWeek == 2)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Monday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 4)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Tuesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 8)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Wednesday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 16)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Thursday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 32)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Friday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                    if (day.DayOfWeek == 64)
                    {
                        schedules.Add(new DailySchedule
                        {
                            DayOfWeek = "Saturday",
                            EndTime = slot.EndTime,
                            StartTime = slot.StartTime,
                        });
                    }
                }
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
                    Price = await GetDynamicPrice(course.Id,false),
                    SyllabusCode = syllabusCode,
                    SyllabusName = syllabusName,
                    SyllabusType = syllabusType,
                    Status = course.Status
                };
                myClassResponse.CourseResponse = customCourseResponse;
                result.Add(myClassResponse);
            }
            if(result.Count == 0)
            {
                return result;
            }
            List<MyClassResponse> finalResponse = new List<MyClassResponse>();  
            if(dateOfWeeks != null && dateOfWeeks.Count > 0)
            {
                foreach(var r in result)
                {
                    var sch = r.Schedules;
                    foreach(var d in dateOfWeeks)
                    {
                        var schList = sch.Select(x => x.DayOfWeek).ToList();
                        var isExist = schList.Any(x => x.ToLower().Equals(d.ToLower()));
                        if(isExist)
                        {
                            finalResponse.Add(r);
                            break;
                        }
                    }
                }
                return finalResponse;
            }
            return result;

        }
        #endregion
    }
}
