﻿using AutoMapper;
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

            filteredCourses = subject != null
            ? filteredCourses.Where(x => x.CourseCategory!.Name!.ToLower().Equals(subject.ToLower())).ToList()
            : filteredCourses;

            return filteredCourses;
        }

        public async Task<CourseResExtraInfor> GetCourseByIdAsync(Guid id)
        {
            var course = await _unitOfWork.GetRepository<Course>().GetListAsync(predicate: x => x.Id == id, include: x => x
            .Include(x => x.CoursePrerequisites)
            .Include(x => x.CourseCategory)
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
            .Include(x => x.CourseCategory)
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
                .Include(x => x.CourseCategory)
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
                    .SingleOrDefaultAsync(predicate: x => x.CoursePrerequisites.Any(cp => cp.PrerequisiteCourseId == c.Id),
                    include: x => x.Include(x => x.CourseCategory));

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

        public async Task<bool> AddCourse(OverallSyllabusRequest request)
        {
            if(request != null) {
                CourseSyllabus syllabus = new CourseSyllabus
                {
                    Id = Guid.NewGuid(),
                    EffectiveDate = DateTime.Parse(request.EffectiveDate),
                    UpdateTime = DateTime.Now,
                    Description = request.Description,
                    MinAvgMarkToPass = request.MinAvgMarkToPass,
                    Name = request.SyllabusName,
                    ScoringScale = request.ScoringScale,
                    StudentTasks = request.StudentTasks,
                    SubjectCode = request.SubjectCode,
                    SyllabusLink = request.SyllabusLink,
                    TimePerSession = request.TimePerSession,
                };
                List<Material> materials = new List<Material>();
                var mat = request.MaterialRequests;
                foreach(var material in mat)
                {
                    materials.Add(new Material
                    {
                        Id = Guid.NewGuid(),
                        CourseSyllabusId = syllabus.Id,
                        URL = material
                    });
                }
                syllabus.Materials = materials;
                List<Topic> topicList = new List<Topic>();
                List<Session> sessions = new List<Session>();
                List<SessionDescription> sessionDescriptions = new List<SessionDescription>();
                var topics = request.SyllabusRequests.ToList();
                foreach(var topicx in topics)
                {
                    Topic topic = new Topic
                    {
                        Id = Guid.NewGuid(),
                        CourseSyllabusId = syllabus.Id,
                        Name = topicx.TopicName,
                        OrderNumber = topicx.Index,
                    };
                    topicList.Add(topic);
                    var sessionList = topicx.SessionRequests.ToList();
                    foreach (var session in sessionList)
                    {
                        var content = session.SessionContentRequests.ToList();
                        Session session1 = new Session
                        {
                            Id = Guid.NewGuid(),
                            TopicId = topic.Id,
                            NoSession = session.Order,
                        };
                        sessions.Add(session1);
                        foreach (var cont in content)
                        {
                            var detailStrings = cont.SessionContentDetails;
                            var stringFinal = "";
                            foreach(var d in detailStrings)
                            {
                                stringFinal = stringFinal + "/r/n" + d.ToString();
                            }
                                SessionDescription sessionDescription = new SessionDescription
                                {
                                    Id = Guid.NewGuid(),
                                    Detail = stringFinal ,
                                    SessionId = session1.Id,
                                    Content = cont.Content,
                                };
                                sessionDescriptions.Add(sessionDescription);
                        }
                    }
                }
                var questionPack = request.QuestionPackageRequests.ToList();
                List<QuestionPackage> questionPackages = new List<QuestionPackage>();
                List<Question> questionList = new List<Question>();
                List<MutipleChoiceAnswer> mutipleChoiceAnswers = new List<MutipleChoiceAnswer>();
                List<FlashCard> flashCards = new List<FlashCard>(); 
                List<SideFlashCard> sideFlashCards = new List<SideFlashCard>(); 
                foreach(var ques in questionPack)
                {
                    var order = ques.NoOfSession;
                    var sessionFound = sessions.SingleOrDefault(x => x.NoSession == order);
                    QuestionPackage qp = new QuestionPackage
                    {
                        Id = Guid.NewGuid(),
                        SessionId = sessionFound.Id,
                        Title = ques.Title,
                        Type = ques.Type,
                    };
                    questionPackages.Add(qp);
                    var questionRequest = ques.QuestionRequests;
                    foreach(var question in questionRequest)
                    {
                        Question question1 = new Question
                        {
                            Id = Guid.NewGuid(),
                            Description = question.Description,
                            Img = question.Img,
                            QuestionPacketId = qp.Id,
                        };
                        questionList.Add(question1);
                        if(question.MutipleChoiceAnswerRequests != null)
                        {
                            var mul = question.MutipleChoiceAnswerRequests;
                            foreach(var answer in mul)
                            {
                                mutipleChoiceAnswers.Add(new MutipleChoiceAnswer
                                {
                                    Description = answer.Description,
                                    Img = answer.Img,
                                    QuestionId = question1.Id,
                                    Score = answer.Score,
                                });
                            } 
                        }
                        if(question.FlashCardRequests != null)
                        {
                            var flashcard = question.FlashCardRequests;
                            foreach(var flash in flashcard)
                            {
                                List<SideFlashCard> flashCardsx = new List<SideFlashCard>();
                                FlashCard flashCard = new FlashCard
                                {
                                    Id = Guid.NewGuid(),
                                    QuestionId = question1.Id,
                                    Score = flash.Score,
                                };
                                flashCards.Add(flashCard);
                                SideFlashCard flashCard1 = new SideFlashCard
                                {
                                    Id = Guid.NewGuid(),
                                    Description = flash.RightSideDescription,
                                    Image = flash.RightSideImg,
                                    Side = "Right",
                                    FlashCardId = flashCard.Id,
                                };
                                flashCardsx.Add(flashCard1);
                                SideFlashCard flashCard2 = new SideFlashCard
                                {
                                    Id = Guid.NewGuid(),
                                    Description = flash.LeftSideDescription,
                                    Image = flash.LeftSideDescription,
                                    Side = "Left",
                                    FlashCardId = flashCard.Id,
                                };
                                flashCardsx.Add(flashCard2);
                                sideFlashCards.AddRange(flashCardsx);
                            }
                        }
                    }
                }
                var examSyll = request.ExamSyllabusRequests;
                List<ExamSyllabus> examSyllab = new List<ExamSyllabus>();
                foreach (var exam in examSyll)
                {
                    ExamSyllabus ex = new ExamSyllabus
                    {
                        Category = exam.Type,
                        CompleteionCriteria = exam.CompleteionCriteria,
                        CourseSyllabusId = syllabus.Id,
                        Duration = exam.Duration,
                        Id = Guid.NewGuid(),
                        QuestionType = exam.QuestionType,
                        Weight = exam.Weight,
                        Part = exam.Part,
                    };
                    examSyllab.Add(ex);
                }
                await _unitOfWork.GetRepository<CourseSyllabus>().InsertAsync(syllabus);
                await _unitOfWork.GetRepository<Material>().InsertRangeAsync(materials);
                await _unitOfWork.GetRepository<Topic>().InsertRangeAsync(topicList);
                await _unitOfWork.GetRepository<Session>().InsertRangeAsync(sessions);
                await _unitOfWork.GetRepository<SessionDescription>().InsertRangeAsync(sessionDescriptions);
                await _unitOfWork.GetRepository<ExamSyllabus>().InsertRangeAsync(examSyllab);
                try
                {
                    bool isSuc = await _unitOfWork.CommitAsync() > 0;
                }
                catch (Exception ex)
                {
                    var msg = ex.InnerException;
                }
                await _unitOfWork.GetRepository<QuestionPackage>().InsertRangeAsync(questionPackages);
                await _unitOfWork.GetRepository<Question>().InsertRangeAsync(questionList);

                try
                {
                    bool isSuc2 = await _unitOfWork.CommitAsync() > 0;
                }
                catch (Exception ex)
                {
                    var msg = ex.InnerException;
                }
                await _unitOfWork.CommitAsync();
                if (mutipleChoiceAnswers != null && mutipleChoiceAnswers.Count > 0)
                {
                    await _unitOfWork.GetRepository<MutipleChoiceAnswer>().InsertRangeAsync(mutipleChoiceAnswers);
                }
                if(flashCards.Count > 0 && flashCards != null) 
                {
                    await _unitOfWork.GetRepository<FlashCard>().InsertRangeAsync(flashCards);
                }
                if(sideFlashCards.Count > 0 && sideFlashCards != null) {
                    await _unitOfWork.GetRepository<SideFlashCard>().InsertRangeAsync(sideFlashCards);
                }
                var isSucess = await _unitOfWork.CommitAsync() > 0;
                return isSucess;
            }
            return false;
        }

        public async Task<bool> AddCourseInformation(CreateCourseRequest request)
        {
            if(request != null)
            {
                Course course = new Course
                {
                    AddedDate = DateTime.Now,
                    CourseCategoryId = Guid.Parse(request.CourseCategoryId),
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
                course.SubDescriptionTitles = subDescriptionTitles;
                var syll = await _unitOfWork.GetRepository<CourseSyllabus>().SingleOrDefaultAsync(predicate :  x => x.Id.ToString().Equals(request.SyllabusId));
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
