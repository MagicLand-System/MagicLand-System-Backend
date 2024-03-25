using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain.Models.TempEntity.Class;
using MagicLand_System.Domain.Models.TempEntity.Quiz;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Request.Syllabus;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
using MagicLand_System.PayLoad.Response.Quizzes.Staff;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.PayLoad.Response.Syllabuses.ForStaff;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MagicLand_System.Services.Implements
{
    public class SyllabusService : BaseService<SyllabusService>, ISyllabusService
    {
        public SyllabusService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<SyllabusService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        #region thanh_lee code
        public async Task<bool> AddSyllabus(OverallSyllabusRequest request)
        {
            if (request != null)
            {
                try
                {
                    var syllabus = await GenerateSyllabus(request);

                    SettingQuestionPackageRequest(request);

                    await GenerateSyllabusItems(request, syllabus.Id);

                    return await _unitOfWork.CommitAsync() > 0;
                }
                catch (Exception ex)
                {
                    throw new BadHttpRequestException($"{ex.Message + ex.InnerException}", StatusCodes.Status400BadRequest);
                }
            }
            return false;
        }

        private void SettingQuestionPackageRequest(OverallSyllabusRequest request)
        {
            var offlineExams = request.ExamSyllabusRequests
            .Where(exam => exam.Method.Trim().ToLower() == "offline")
            .ToList();

            if (offlineExams.Any())
            {
                var sessionsRequest = request.SyllabusRequests
               .SelectMany(sr => sr.SessionRequests)
               .ToList();

                request.QuestionPackageRequests!.AddRange(
                    from session in sessionsRequest
                    from content in session.SessionContentRequests
                    join exam in offlineExams on content.Content.Trim().ToLower() equals exam.ContentName.Trim().ToLower()
                    select new QuestionPackageRequest
                    {
                        ContentName = exam.ContentName,
                        NoOfSession = session.Order,
                        Type = "options",
                        Title = "Làm Tại Nhà",
                        Score = 0,
                        Duration = 0,
                    });
            }
        }

        public async Task<string> CheckingSyllabusInfor(string name, string code)
        {
            string message = "Thông Tin Giáo Trình Hợp Lệ";
            int invalid = 0;

            var syllabuses = (await _unitOfWork.GetRepository<Syllabus>().GetListAsync()).ToList();
            var syllabusesSubjectCode = syllabuses.Select(syll => syll.SubjectCode!.Substring(0, syll.SubjectCode.Length - 2)).ToList();
            if (syllabusesSubjectCode.Any(ssc => StringHelper.TrimStringAndNoSpace(ssc!) == StringHelper.TrimStringAndNoSpace(code)))
            {
                message = $"Mã Giáo Trình Đã Tồn Tại";
                invalid++;

            }

            var syllabusesName = syllabuses.Select(syll => StringHelper.TrimStringAndNoSpace(syll.Name!).ToLower()).ToList();
            if (syllabusesName.Any(sn => sn == StringHelper.TrimStringAndNoSpace(name).ToLower()))
            {
                message = $"Tên Giáo Trình Đã Tồn Tại";
                invalid++;
            }
            if (invalid >= 2)
            {
                message = $"Tên Giáo Trình Và Mã Giáo Trình Đã Tồn Tại";
            }
            return message;
        }
        private async Task<Syllabus> GenerateSyllabus(OverallSyllabusRequest request)
        {
            var syllabuses = (await _unitOfWork.GetRepository<Syllabus>().GetListAsync()).ToList();

            ValidateSyllabus(request, syllabuses);

            Guid newSyllabusId = Guid.NewGuid();

            var syllabus = new Syllabus
            {
                Id = newSyllabusId,
                UpdateTime = DateTime.Now,
                Description = request.Description,
                MinAvgMarkToPass = request.MinAvgMarkToPass,
                Name = request.SyllabusName,
                ScoringScale = request.ScoringScale,
                StudentTasks = request.StudentTasks,
                SubjectCode = request.SubjectCode + "01",
                SyllabusLink = request.SyllabusLink,
                TimePerSession = request.TimePerSession,
                NumOfSessions = request.NumOfSessions,
            };

            if (request.EffectiveDate != null)
            {
                string format = "dd/MM/yyyy";

                var date = DateTime.TryParseExact(request.EffectiveDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                    ? (DateTime?)parsedDate
                    : DateTime.Parse(request.EffectiveDate);

                syllabus.EffectiveDate = date;

            }

            if (request.PreRequisite != null && request.PreRequisite.Any())
            {
                var syllabusPrerequisties = new List<SyllabusPrerequisite>();

                foreach (string code in request.PreRequisite!)
                {
                    var syllabusId = await _unitOfWork.GetRepository<Syllabus>()
                   .SingleOrDefaultAsync(selector: x => x.Id, predicate: x => x.SubjectCode!.ToLower().Trim().Equals(code!.ToLower().Trim()));
                    if (syllabusId == default)
                    {
                        throw new BadHttpRequestException($"Mã Giáo Trình Tiên Quyết Không Tồn Tại [{code}]", StatusCodes.Status400BadRequest);
                    }

                    syllabusPrerequisties.Add(new SyllabusPrerequisite
                    {
                        Id = Guid.NewGuid(),
                        CurrentSyllabusId = newSyllabusId,
                        PrerequisiteSyllabusId = syllabusId,
                    });
                }

                await _unitOfWork.GetRepository<SyllabusPrerequisite>().InsertRangeAsync(syllabusPrerequisties);
            }

            var categoryId = await _unitOfWork.GetRepository<SyllabusCategory>()
               .SingleOrDefaultAsync(selector: x => x.Id, predicate: x => x.Name!.ToLower().Trim().Equals(request.Type!.ToLower().Trim()));
            syllabus.SyllabusCategoryId = categoryId;


            syllabus.Materials = request.MaterialRequests.Select(mat => new Material
            {
                Id = new Guid(),
                SyllabusId = newSyllabusId,
                URL = mat.URL,
                Name = mat.FileName,
            }).ToList();

            await _unitOfWork.GetRepository<Syllabus>().InsertAsync(syllabus);
            await _unitOfWork.GetRepository<Material>().InsertRangeAsync(syllabus.Materials);
            return syllabus;
        }

        private void ValidateSyllabus(OverallSyllabusRequest request, List<Syllabus> syllabuses)
        {
            var syllabusesSubjectCode = syllabuses.Select(syll => syll.SubjectCode!.Substring(0, syll.SubjectCode.Length - 2)).ToList();
            if (syllabusesSubjectCode.Any(ssc => StringHelper.TrimStringAndNoSpace(ssc!) == StringHelper.TrimStringAndNoSpace(request.SubjectCode!)))
            {
                throw new BadHttpRequestException($"Mã Giáo Trình [{request.SubjectCode!}] Đã Tồn Tại", StatusCodes.Status400BadRequest);
            }

            var syllabusesName = syllabuses.Select(syll => StringHelper.TrimStringAndNoSpace(syll.Name!)).ToList();
            if (syllabusesName.Any(sn => sn == StringHelper.TrimStringAndNoSpace(request.SyllabusName!)))
            {
                throw new BadHttpRequestException($"Tên Giáo Trình [{request.SyllabusName!}] Đã Tồn Tại", StatusCodes.Status400BadRequest);
            }
        }

        private async Task GenerateSyllabusItems(OverallSyllabusRequest request, Guid syllabusId)
        {
            var sessions = await GenerateLearningItems(request.SyllabusRequests, syllabusId);
            await GenerateExerciseItems(request.QuestionPackageRequests!, sessions);
            await GenerateExam(request, syllabusId);
        }

        private async Task GenerateExam(OverallSyllabusRequest request, Guid syllabusId)
        {
            var examList = request.ExamSyllabusRequests.Select(exam => new ExamSyllabus
            {
                Category = exam.Type,
                ContentName = exam.ContentName,
                CompleteionCriteria = exam.CompleteionCriteria,
                SyllabusId = syllabusId,
                QuestionType = exam.QuestionType,
                Weight = exam.Weight,
                Part = exam.Part,
                Method = exam.Method,
                Duration = exam.Duration,
            }).ToList();

            await _unitOfWork.GetRepository<ExamSyllabus>().InsertRangeAsync(examList);
        }

        private async Task<List<Session>> GenerateLearningItems(List<SyllabusRequest> syllabusRequest, Guid syllabusId)
        {
            var sessionList = new List<Session>();
            foreach (var tp in syllabusRequest)
            {
                Guid newTopicId = Guid.NewGuid();
                var topic = new Topic
                {
                    Id = newTopicId,
                    SyllabusId = syllabusId,
                    Name = tp.TopicName,
                    OrderNumber = tp.Index,
                };

                await _unitOfWork.GetRepository<Topic>().InsertAsync(topic);
                sessionList.AddRange(await GenerateSession(tp.SessionRequests, newTopicId));
            }

            await _unitOfWork.GetRepository<Session>().InsertRangeAsync(sessionList);
            return sessionList;
        }

        private async Task<List<Session>> GenerateSession(List<SessionRequest> sessionRequest, Guid newTopicId)
        {
            try
            {
                var sessionList = new List<Session>();
                var sessionDescriptionList = new List<SessionDescription>();

                foreach (var ses in sessionRequest)
                {
                    Guid newSessionId = Guid.NewGuid();
                    sessionList.Add(new Session
                    {
                        Id = newSessionId,
                        TopicId = newTopicId,
                        NoSession = ses.Order,
                    });

                    int orderContent = 0;
                    foreach (var cont in ses.SessionContentRequests)
                    {
                        orderContent++;
                        var detailStrings = cont.SessionContentDetails.ToArray();
                        var stringFinal = "";
                        for (int i = 0; i < detailStrings.Length; i++)
                        {
                            if (i != 0)
                            {
                                stringFinal += "/r/n";
                            }
                            stringFinal += detailStrings[i];
                        }

                        sessionDescriptionList.Add(new SessionDescription
                        {
                            Id = Guid.NewGuid(),
                            Detail = stringFinal,
                            SessionId = newSessionId,
                            Content = cont.Content,
                            Order = orderContent,
                        });
                    }
                }

                await _unitOfWork.GetRepository<Session>().InsertRangeAsync(sessionList);
                await _unitOfWork.GetRepository<SessionDescription>().InsertRangeAsync(sessionDescriptionList);

                return sessionList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task GenerateExerciseItems(List<QuestionPackageRequest> questionPackageRequest, List<Session> sessions)
        {
            int orderPackage = 0;
            foreach (var qp in questionPackageRequest.OrderBy(qp => qp.NoOfSession).ToList())
            {
                orderPackage++;
                Guid newQuestionPackageId = Guid.NewGuid();
                await GenerateQuestionPackage(sessions, qp, newQuestionPackageId, orderPackage);
                if (qp.QuestionRequests != null)
                {
                    await GenerateQuestionPackgeItems(newQuestionPackageId, qp.QuestionRequests);
                }
            }

        }

        private async Task GenerateQuestionPackage(List<Session> sessions, QuestionPackageRequest qp, Guid newQuestionPackageId, int orderPackage)
        {
            try
            {
                var order = qp.NoOfSession;
                var sessionFound = sessions.SingleOrDefault(x => x.NoSession == order);

                var questionPackage = new QuestionPackage
                {
                    Id = newQuestionPackageId,
                    SessionId = sessionFound!.Id,
                    Title = qp.Title,
                    ContentName = qp.ContentName,
                    Type = qp.Type,
                    Score = qp.Score,
                    OrderPackage = orderPackage,
                    NoSession = qp.NoOfSession,
                    Duration = qp.Duration,
                };

                await _unitOfWork.GetRepository<QuestionPackage>().InsertAsync(questionPackage);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task GenerateQuestionPackgeItems(Guid newQuestionPackageId, List<QuestionRequest> questionRequest)
        {
            foreach (var question in questionRequest)
            {
                Guid newQuestionId = Guid.NewGuid();
                await GenerateQuestion(newQuestionPackageId, question, newQuestionId);
                await GenerateMutipleChoice(question, newQuestionId);

                if (question.FlashCardRequests != null)
                {
                    await GenerateFlashCardItems(question, newQuestionId);
                }
            }

        }

        private async Task GenerateFlashCardItems(QuestionRequest quest, Guid newQuestionId)
        {
            var flashCardList = new List<FlashCard>();
            var sideFlashCardList = new List<SideFlashCard>();

            foreach (var flash in quest.FlashCardRequests!)
            {
                Guid newFlashCardId = Guid.NewGuid();
                flashCardList.Add(new FlashCard
                {
                    Id = newFlashCardId,
                    QuestionId = newQuestionId,
                    Score = flash.Score,
                });

                sideFlashCardList.Add(new SideFlashCard
                {
                    Description = flash.RightSideDescription,
                    Image = flash.RightSideImg,
                    Side = "Right",
                    FlashCardId = newFlashCardId,
                });

                sideFlashCardList.Add(new SideFlashCard
                {
                    Description = flash.LeftSideDescription,
                    Image = flash.LeftSideImg,
                    Side = "Left",
                    FlashCardId = newFlashCardId,
                });
            }

            if (flashCardList.Any())
            {
                await _unitOfWork.GetRepository<FlashCard>().InsertRangeAsync(flashCardList);
                await _unitOfWork.CommitAsync();
            }
            if (sideFlashCardList.Any())
            {
                await _unitOfWork.GetRepository<SideFlashCard>().InsertRangeAsync(sideFlashCardList);
                await _unitOfWork.CommitAsync();
            }

        }

        private async Task GenerateQuestion(Guid newQuestionPackageId, QuestionRequest quest, Guid newQuestionId)
        {
            var question = new Question
            {
                Id = newQuestionId,
                Description = quest.Description,
                Img = quest.Img,
                QuestionPacketId = newQuestionPackageId,
            };
            await _unitOfWork.GetRepository<Question>().InsertAsync(question);
            await _unitOfWork.CommitAsync();
        }

        private async Task GenerateMutipleChoice(QuestionRequest quest, Guid newQuestionId)
        {
            var mutipleChoiceAnswers = new List<MultipleChoice>();
            if (quest.MutipleChoiceAnswerRequests != null)
            {
                foreach (var answer in quest.MutipleChoiceAnswerRequests)
                {
                    mutipleChoiceAnswers.Add(new MultipleChoice
                    {
                        Description = answer.Description,
                        Img = answer.Img,
                        QuestionId = newQuestionId,
                        Score = answer.Score,
                    });
                }
            }

            if (mutipleChoiceAnswers != null && mutipleChoiceAnswers.Count > 0)
            {
                await _unitOfWork.GetRepository<MultipleChoice>().InsertRangeAsync(mutipleChoiceAnswers);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task<List<SyllabusResponse>> FilterSyllabusAsync(List<string>? keyWords, DateTime? date, double? score)
        {
            score ??= double.MaxValue;

            var syllabuses = await FetchAllSyllabus();

            syllabuses = keyWords == null || keyWords.Count() == 0
                ? syllabuses
                : syllabuses.Where(syll =>
                    keyWords.Any(k =>
                        (k != null) &&
                        (syll.Name != null && syll.Name!.ToLower().Contains(k.ToLower()) ||
                         syll.SubjectCode != null && syll.SubjectCode!.ToString().ToLower().Contains(k.ToLower()) ||
                         syll.ScoringScale >= score ||
                         syll.MinAvgMarkToPass >= score)
                    )
                ).ToList();

            if (date != default && date != null)
            {
                syllabuses = syllabuses.Where(syll => syll.UpdateTime.Date == date || syll.EffectiveDate != null && syll.EffectiveDate.Value.Date == date).ToList();
            }
            syllabuses = syllabuses.OrderByDescending(x => x.UpdateTime).ToList();

            return syllabuses.Select(syll => _mapper.Map<SyllabusResponse>(syll)).OrderByDescending(x => x.UpdateDate).ToList();
        }


        public async Task<SyllabusResponse> LoadSyllabusDynamicIdAsync(Guid courseId, Guid classId)
        {
            var syllabus = await ValidateSyllabus(courseId, true);

            if (classId != default)
            {
                var cls = await ValidateClassForSchedule(courseId, classId);
                var response = SyllabusCustomMapper.fromSyllabusAndClassToSyllabusResponseWithSheduleResponse(syllabus, cls);
                foreach (var session in response.SyllabusInformations!.Topics!.SelectMany(tp => tp.Sessions!).ToList())
                {
                    var quiz = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(
                        predicate: x => x.NoSession == session.OrderSession);

                    if (quiz != null)
                    {
                        var quizTime = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(
                        predicate: x => x.ExamId == quiz.Id && x.ClassId == classId);

                        var startTime = DateTime.Parse(session.Date!).Date.Add(session.StartTime!.Value.ToTimeSpan());
                        var endTime = DateTime.Parse(session.Date!).Date.Add(session.EndTime!.Value.ToTimeSpan());

                        session.Quiz = new QuizInforResponse
                        {
                            ExamId = quiz.Id,
                            ExamName = "Bài Kiểm Tra Số " + quiz.OrderPackage,
                            ExamPart = quiz.Type!.Trim().ToLower() == QuizTypeEnum.flashcard.ToString() ? 2 : 1,
                            QuizName = quiz.Title!,
                            Attempts = quizTime != null ? quizTime.AttemptAllowed : 1,
                            QuizDuration = quiz.Duration != null ? quiz.Duration.Value : 300,
                            QuizStartTime = quizTime != null && quizTime.ExamStartTime != default ? startTime.Date.Add(quizTime.ExamStartTime) : startTime,
                            QuizEndTime = quizTime != null && quizTime.ExamEndTime != default ? endTime.Date.Add(quizTime.ExamEndTime) : endTime,
                        };
                    }
                }
                return response;
            }

            return _mapper.Map<SyllabusResponse>(syllabus);
        }

        private async Task<Class> ValidateClassForSchedule(Guid courseId, Guid classId)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == classId,
                include: x => x.Include(x => x.Schedules.OrderBy(sch => sch.Date)).ThenInclude(sch => sch.Slot)!);

            if (cls == null || cls.CourseId != courseId)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại Hoặc Không Thuộc Về Khóa Học Có Id [{courseId}]", StatusCodes.Status400BadRequest);
            }

            if (cls.Status == ClassStatusEnum.CANCELED.ToString())
            {
                throw new BadHttpRequestException($"Id [{classId}] Này Của Lớp Đã Hủy, Không Thể Truy Suất Kèm Lịch Học Của Lớp", StatusCodes.Status400BadRequest);
            }

            return cls;
        }

        private async Task<Syllabus> ValidateSyllabus(Guid id, bool isCourseId)
        {
            var syllabus = await CheckingIdRequest(id, isCourseId);

            if (syllabus == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Khóa Học Hoặc Giáo Trình Không Tồn Tại, Hoặc Id Khóa Học Không Thuộc Về Bất Cứ Giáo Trình Nào", StatusCodes.Status400BadRequest);
            };

            foreach (var session in syllabus.Topics!.SelectMany(tp => tp.Sessions!).ToList())
            {
                session.QuestionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id);
            }

            return syllabus;
        }

        private async Task<Syllabus> CheckingIdRequest(Guid id, bool isCourseId)
        {
            var syllabus = new Syllabus();

            if (isCourseId)
            {
                syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.CourseId == id,
                include: x => x.Include(x => x.Materials)
               .Include(x => x.SyllabusCategory)
               .Include(x => x.ExamSyllabuses!));
            }
            else
            {
                syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id == id,
                include: x => x.Include(x => x.Materials)
               .Include(x => x.SyllabusCategory)
               .Include(x => x.ExamSyllabuses!));
            }

            syllabus.Course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.SyllabusId == syllabus.Id);

            syllabus.Topics = await _unitOfWork.GetRepository<Topic>().GetListAsync(
            predicate: x => x.SyllabusId == syllabus.Id,
            orderBy: x => x.OrderBy(x => x.OrderNumber),
            include: x => x.Include(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.SessionDescriptions!.OrderBy(sd => sd.Order)));

            return syllabus;

        }

        public async Task<SyllabusResponse> LoadSyllabusByIdAsync(Guid id)
        {
            var syllabus = await ValidateSyllabus(id, false);

            return _mapper.Map<SyllabusResponse>(syllabus);

        }

        public async Task<List<SyllabusResponse>> LoadSyllabusesAsync()
        {
            var syllabuses = await FetchAllSyllabus();
            return syllabuses.Select(syll => _mapper.Map<SyllabusResponse>(syll)).ToList();
        }

        private async Task<List<Syllabus>> FetchAllSyllabus()
        {
            var syllabuses = await _unitOfWork.GetRepository<Syllabus>().GetListAsync(orderBy: x => x.OrderBy(x => x.UpdateTime),
               include: x => x.Include(x => x.Materials)
               .Include(x => x.SyllabusCategory)
               .Include(x => x.ExamSyllabuses!));

            foreach (var syll in syllabuses)
            {
                syll.Course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.SyllabusId == syll.Id);

                syll.Topics = await _unitOfWork.GetRepository<Topic>().GetListAsync(
                predicate: x => x.SyllabusId == syll.Id,
                orderBy: x => x.OrderBy(x => x.OrderNumber),
                include: x => x.Include(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.SessionDescriptions!.OrderBy(sd => sd.Order)));

                foreach (var session in syll.Topics.SelectMany(tp => tp.Sessions!))
                {
                    session.QuestionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id);
                }
            }

            string role = GetRoleFromJwt();
            if (role.ToLower() == RoleEnum.LECTURER.ToString().ToLower())
            {
                var coursesOfLecturer = await _unitOfWork.GetRepository<Course>().GetListAsync(
                    selector: x => x.Id,
                    predicate: x => x.Classes.Any(cls => cls.LecturerId == GetUserIdFromJwt()));

                syllabuses = syllabuses.Where(syll => coursesOfLecturer.Any(id => id == syll.CourseId)).ToList();
            }

            if (role.ToLower() == RoleEnum.STUDENT.ToString().ToLower())
            {
                var coursesOfStudent = await _unitOfWork.GetRepository<Class>().GetListAsync(
                    selector: x => x.CourseId,
                    predicate: x => x.StudentClasses.Any(sc => sc.StudentId == GetUserIdFromJwt()));

                syllabuses = syllabuses.Where(syll => coursesOfStudent.Any(id => id == syll.CourseId)).ToList();
            }

            return syllabuses.ToList();
        }

        public async Task<List<ExamWithQuizResponse>> LoadQuizzesAsync()
        {
            var quizzesResponse = new List<ExamWithQuizResponse>();

            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(
               include: x => x.Include(x => x.Syllabus!)
              .ThenInclude(syll => syll.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
              .Include(x => x.Syllabus).ThenInclude(syll => syll!.ExamSyllabuses!));

            await GenerateQuizzesResponse(quizzesResponse, courses);

            return quizzesResponse;
        }

        private async Task GenerateQuizzesResponse(List<ExamWithQuizResponse> quizzesResponse, ICollection<Course> courses)
        {
            foreach (var course in courses)
            {
                if (course.Syllabus == null)
                {
                    continue;
                }

                var sessions = course.Syllabus!.Topics!.SelectMany(tp => tp.Sessions!).ToList();

                foreach (var session in sessions)
                {
                    await GenerateQuizzes(quizzesResponse, course, session);
                }
            }
        }

        private async Task GenerateQuizzes(List<ExamWithQuizResponse> quizzesResponse, Course course, Session session)
        {
            var questionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id,
                include: x => x.Include(x => x.Questions!).ThenInclude(quest => quest.MutipleChoices!)
                .Include(x => x.Questions!).ThenInclude(quest => quest.FlashCards!).ThenInclude(fc => fc.SideFlashCards!));

            if (questionPackage == null)
            {
                return;
            }

            var exam = course.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => StringHelper.TrimStringAndNoSpace(exam.ContentName!) == StringHelper.TrimStringAndNoSpace(questionPackage.ContentName!));
            var quizResponse = QuizCustomMapper.fromSyllabusItemsToQuizWithQuestionResponse(session.NoSession, questionPackage, exam);
            quizResponse.SessionId = session.Id;
            quizResponse.CourseId = course.Id;
            quizResponse.Date = "Cần Truy Suất Qua Lớp";

            quizzesResponse.Add(quizResponse);

            //GenerateQuizMutipleChoice(quizzesResponse, course, session, questionPackage);

            //GenerateQuizFlashCard(quizFlashCards, course, session, questionPackage);
        }
        #region 
        //private void GenerateQuizFlashCard(List<QuizFlashCardResponse> quizFlashCards, Course course, Session session, QuestionPackage questionPackage)
        //{
        //    if (questionPackage.Questions!.SelectMany(quest => quest.FlashCards!).Any())
        //    {
        //        var exam = course.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => exam.ExamOrder == questionPackage.PackageOrder);
        //        var responseFlashCard = QuizCustomMapper.fromSyllabusItemsToQuizFlashCardResponse(session.NoSession, questionPackage, exam!);
        //        responseFlashCard.SessionId = session.Id;
        //        responseFlashCard.CourseId = course.Id;

        //        quizFlashCards.Add(responseFlashCard);
        //    }
        //}

        //private void GenerateQuizMutipleChoice(List<QuizResponse> quizzesResponse, Course course, Session session, QuestionPackage questionPackage)
        //{
        //    if (questionPackage.Questions!.SelectMany(quest => quest.MutipleChoiceAnswers!).Any())
        //    {
        //        var exam = course.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => exam.ExamOrder == questionPackage.PackageOrder);
        //        var responseMutipleChoice = QuizCustomMapper.fromSyllabusItemsToQuizMutipleChoiceResponse(session.NoSession, questionPackage, exam!);
        //        responseMutipleChoice.SessionId = session.Id;
        //        responseMutipleChoice.CourseId = course.Id;

        //        quizzesResponse.Add(responseMutipleChoice);
        //    }
        //}
        #endregion
        public async Task<List<ExamWithQuizResponse>> LoadQuizzesByCourseIdAsync(Guid id)
        {
            //var quizMultipleChoices = new List<QuizMultipleChoiceResponse>();
            //var quizFlashCards = new List<QuizFlashCardResponse>();

            var quizzesResponse = new List<ExamWithQuizResponse>();

            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(
                predicate: x => x.Id == id,
                include: x => x.Include(x => x.Syllabus!).ThenInclude(syll => syll.Topics!.OrderBy(tp => tp.OrderNumber))
               .ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
               .Include(x => x.Syllabus).ThenInclude(syll => syll!.ExamSyllabuses!));

            if (!courses.Any())
            {
                throw new BadHttpRequestException($"Id [{id}] Của Khóa Học Không Tồn Tại Hoặc Khóa Học Không Thuộc Về Bất Cứ Giáo Trình Nào", StatusCodes.Status400BadRequest);
            }

            await GenerateQuizzesResponse(quizzesResponse, courses);

            return quizzesResponse;
        }

        private async Task<Class> ValidateClass(Guid classId, Guid? studentId)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == classId,
                include: x => x.Include(x => x.Course).Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)!);

            if (studentId != null && studentId != default)
            {
                var studentClass = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(predicate: x => x.ClassId == classId && x.StudentId == studentId.Value);
                if (studentClass is null)
                {
                    throw new BadHttpRequestException($"Id [{studentId}] Của Học Sinh Không Thuộc Lớp Học Đang Truy Suất", StatusCodes.Status400BadRequest);
                }
            }

            cls.Course!.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
            predicate: x => x.CourseId == cls.CourseId,
            include: x => x.Include(syll => syll!.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))!.Include(syll => syll!.ExamSyllabuses!));

            if (cls == null || cls.Course!.Syllabus == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại Hoặc Thuộc Khóa Học Không Thuộc Về Bất Cứ Giáo Trình Nào", StatusCodes.Status400BadRequest);
            }

            if (cls.Status == ClassStatusEnum.CANCELED.ToString())
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Đã Hủy Không Thể Truy Suất", StatusCodes.Status400BadRequest);
            }

            return cls;
        }

        private async Task GenerateExamWithDate(List<ExamResponse> examsResponse, Class cls, Session session)
        {
            var questionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id,
            include: x => x.Include(x => x.Questions!).ThenInclude(quest => quest.MutipleChoices!)
           .Include(x => x.Questions!).ThenInclude(quest => quest.FlashCards!).ThenInclude(fc => fc.SideFlashCards!));

            if (questionPackage == null)
            {
                return;
            }

            var exam = cls.Course!.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => StringHelper.TrimStringAndNoSpace(exam.ContentName!) == StringHelper.TrimStringAndNoSpace(questionPackage.ContentName!));
            var examResponse = QuizCustomMapper.fromSyllabusItemsToExamResponse(questionPackage, exam);
            var quizTime = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(predicate: x => x.ExamId == questionPackage.Id && x.ClassId == cls.Id);

            var schedule = cls.Schedules.ToList()[session.NoSession - 1];
            var date = schedule.Date.ToString("yyyy-MM-ddTHH:mm:ss");
            var startTime = DateTime.Parse(date).Date.Add(TimeSpan.Parse(schedule.Slot!.StartTime));
            var endTime = DateTime.Parse(date).Date.Add(TimeSpan.Parse(schedule.Slot!.EndTime));

            examResponse.SessionId = session.Id;
            examResponse.CourseId = cls.Course.Id;
            examResponse.Date = date;
            examResponse.AttemptAlloweds = quizTime != null ? quizTime.AttemptAllowed : 1;
            examResponse.ExamStartTime = quizTime != null && quizTime.ExamStartTime != default ? startTime.Date.Add(quizTime.ExamStartTime) : startTime;
            examResponse.ExamEndTime = quizTime != null && quizTime.ExamEndTime != default ? endTime.Date.Add(quizTime.ExamEndTime) : endTime;

            examsResponse.Add(examResponse);
        }

        public async Task<List<ExamExtraInfor>> LoadExamOfCurrentStudentAsync(int numberOfDate)
        {
            var studentId = (await GetUserFromJwt()).StudentIdAccount;
            if (studentId == null)
            {
                throw new BadHttpRequestException("Lỗi Hệ Thống Không Thể Xác Thực Người Dùng, Vui Lòng Đăng Nhập Lại Và Thực Hiện Lại Thao Tác",
                    StatusCodes.Status500InternalServerError);
            }

            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.StudentClasses.Any(sc => sc.StudentId == studentId) && x.Status == ClassStatusEnum.PROGRESSING.ToString());

            if (!classes.Any())
            {
                throw new BadHttpRequestException("Bé Chưa Tham Gia Lớp Học Nào", StatusCodes.Status400BadRequest);
            }
            var responses = new List<ExamExtraInfor>();
            foreach (var cls in classes)
            {
                var examsResponse = new List<ExamResponse>();

                cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                    orderBy: x => x.OrderBy(x => x.Date),
                    predicate: x => x.ClassId == cls.Id,
                    include: x => x.Include(x => x.Slot).Include(x => x.Room)!);


                cls.Course!.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.CourseId == cls.CourseId,
                include: x => x.Include(syll => syll!.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))!.Include(syll => syll!.ExamSyllabuses!));

                foreach (var session in cls.Course!.Syllabus!.Topics!.SelectMany(tp => tp.Sessions!).ToList())
                {
                    await GenerateExamWithDate(examsResponse, cls, session);
                }

                await SettingExamInfor(numberOfDate, studentId.Value, responses, cls, examsResponse);
            }

            return responses;
        }

        private async Task SettingExamInfor(int numberOfDate, Guid studentId, List<ExamExtraInfor> responses, Class cls, List<ExamResponse> examsResponse)
        {
            foreach (var exam in examsResponse)
            {
                string status = string.Empty;
                var examDate = DateTime.Parse(exam.Date!).Date;
                var currentDate = DateTime.Now.Date;

                var test = await _unitOfWork.GetRepository<TestResult>().SingleOrDefaultAsync(predicate: x => x.ExamId == exam.ExamId && x.StudentClass!.StudentId == studentId);
                if (test != null)
                {
                    status = "Đã Hoàn Thành";
                }
                else
                {
                    if (examDate < currentDate)
                    {
                        status = "Hết Hạn Làm Bài";
                    }
                    if (examDate == currentDate)
                    {
                        status = "Hôm Nay";
                    }
                    if (examDate > currentDate)
                    {
                        status = examDate.Day - currentDate.Day + " Ngày Tới";
                    }
                }

                if (examDate >= currentDate.AddDays(-numberOfDate).Date && examDate <= currentDate.AddDays(+numberOfDate).Date)
                {
                    responses.Add(new ExamExtraInfor
                    {
                        ExamId = exam.ExamId,
                        ExamPart = exam.ExamPart,
                        ExamName = exam.ExamName,
                        QuizCategory = exam.QuizCategory,
                        QuizType = exam.QuizType,
                        QuizName = exam.QuizName,
                        Weight = exam.Weight,
                        CompletionCriteria = exam.CompletionCriteria,
                        TotalScore = exam.TotalScore,
                        TotalMark = exam.TotalMark,
                        Date = exam.Date,
                        NoSession = exam.NoSession,
                        RoomName = cls.Schedules.ToList()[exam.NoSession - 1].Room!.Name,
                        SessionId = exam.SessionId,
                        CourseId = exam.CourseId,
                        ClassId = cls.Id,
                        ClassName = cls.ClassCode,
                        Method = cls.Method,
                        Status = status,
                    });
                }
            }
        }

        public async Task<List<ExamResForStudent>> LoadExamOfClassByClassIdAsync(Guid classId, Guid? studentId)
        {
            var examsResponse = new List<ExamResponse>();

            var cls = await ValidateClass(classId, studentId);

            foreach (var session in cls.Course!.Syllabus!.Topics!.SelectMany(tp => tp.Sessions!).ToList())
            {
                await GenerateExamWithDate(examsResponse, cls, session);
            }
            var responses = examsResponse.Select(x => _mapper.Map<ExamResForStudent>(x)).ToList();

            if (studentId != null && studentId != default)
            {
                foreach (var res in responses)
                {
                    var isQuizDone = await _unitOfWork.GetRepository<TestResult>().SingleOrDefaultAsync(
                        orderBy: x => x.OrderByDescending(x => x.NoAttempt),
                        predicate: x => x.StudentClass!.ClassId == classId && x.StudentClass.StudentId == studentId && x.ExamId == res.ExamId);

                    var attemptSetting = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(
                        selector: x => x.AttemptAllowed,
                        predicate: x => x.ExamId == res.ExamId);

                    res.Score = isQuizDone == null ? null : isQuizDone.ScoreEarned;
                    if (attemptSetting != 0)
                    {
                        res.AttemptLeft = isQuizDone != null ? attemptSetting - isQuizDone.NoAttempt : attemptSetting;
                    }
                    else
                    {
                        res.AttemptLeft = isQuizDone != null ? 0 : 1;
                    }

                }
            }

            return responses;
        }

        public async Task<List<QuizResponse>> LoadQuizOfExamByExamIdAsync(Guid examId, int? examPart)
        {
            var quiz = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(
                predicate: x => x.Id == examId,
                include: x => x.Include(x => x.Questions!));

            if (quiz == null)
            {
                throw new BadHttpRequestException($"Id [{examId}] Của Bài Kiểm Tra Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            if (quiz.Score == 0)
            {
                return default!;
            }

            foreach (var question in quiz.Questions!)
            {
                var multipleChoices = await _unitOfWork.GetRepository<MultipleChoice>().GetListAsync(predicate: x => x.QuestionId == question.Id);
                if (multipleChoices.Any())
                {
                    question.MutipleChoices = multipleChoices.ToList();
                    continue;
                }
                var flashCards = await _unitOfWork.GetRepository<FlashCard>().GetListAsync(predicate: x => x.QuestionId == question.Id, include: x => x.Include(x => x.SideFlashCards!));
                if (flashCards.Any())
                {
                    question.FlashCards = flashCards.ToList();
                }
            }

            var responses = QuestionCustomMapper.fromQuestionPackageToQuizResponseInLimitScore(quiz)!;

            await GenereateTempExam(examId, quiz, responses);
            return responses;
        }

        private async Task GenereateTempExam(Guid examId, QuestionPackage quiz, List<QuizResponse> responses)
        {
            try
            {

                int totalMark = 0;
                if (responses.SelectMany(r => r.AnwserFlashCarsInfor!).ToList().Any())
                {
                    totalMark = responses.Sum(r => r.AnwserFlashCarsInfor!.Count()) / 2;
                }
                else
                {
                    totalMark = responses.Count();
                }
                var tempQuestions = new List<TempQuestion>();
                var tempMCAnswers = new List<TempMCAnswer>();
                //var tempFCAnswers = new List<TempFCAnswer>();

                Guid tempQuizId = Guid.NewGuid();
                var tempQuiz = new TempQuiz
                {
                    Id = tempQuizId,
                    ExamId = examId,
                    StudentId = (await GetUserFromJwt()).StudentIdAccount!.Value,
                    TotalMark = totalMark,
                    ExamType = quiz.Type,
                    CreatedTime = DateTime.Now,
                    IsGraded = false,
                };
                foreach (var res in responses)
                {
                    Guid tempQuestionId = Guid.NewGuid();
                    tempQuestions.Add(new TempQuestion
                    {
                        Id = tempQuestionId,
                        QuestionId = res.QuestionId,
                        TempQuizId = tempQuizId,
                    });

                    var multipleChoiceAnswers = res.AnswersMutipleChoicesInfor;
                    if (multipleChoiceAnswers != null && multipleChoiceAnswers.Count > 0)
                    {
                        foreach (var answer in multipleChoiceAnswers)
                        {
                            tempMCAnswers.Add(new TempMCAnswer
                            {
                                Id = Guid.NewGuid(),
                                AnswerId = answer.AnswerId,
                                Score = answer.Score,
                                TempQuestionId = tempQuestionId,
                            });
                        }
                    }
                    //var flashCardAnswers = res.AnwserFlashCarsInfor;
                    //if (flashCardAnswers != null && flashCardAnswers.Count > 0)
                    //{
                    //    foreach (var answer in flashCardAnswers)
                    //    {
                    //        tempFCAnswers.Add(new TempFCAnswer
                    //        {
                    //            Id = Guid.NewGuid(),
                    //            CardId = answer.CardId,
                    //            Score = answer.Score,
                    //            NumberCoupleIdentify = answer.NumberCoupleIdentify,
                    //            TempQuestionId = tempQuestionId,
                    //        });
                    //    }
                    //}
                }

                await _unitOfWork.GetRepository<TempQuiz>().InsertAsync(tempQuiz);
                await _unitOfWork.GetRepository<TempQuestion>().InsertRangeAsync(tempQuestions);
                if (tempMCAnswers.Any())
                {
                    await _unitOfWork.GetRepository<TempMCAnswer>().InsertRangeAsync(tempMCAnswers);
                }
                //if (tempFCAnswers.Any())
                //{
                //    await _unitOfWork.GetRepository<TempFCAnswer>().InsertRangeAsync(tempFCAnswers);
                //}
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status400BadRequest);
            }
        }
        #endregion
        #region thuong code
        public async Task<List<SyllabusResponseV2>> GetAllSyllabus(string? keyword = null)
        {
            var syllabuses = await _unitOfWork.GetRepository<Syllabus>().GetListAsync(include: x => x.Include<Syllabus, Course>(x => x.Course));
            List<SyllabusResponseV2> responses = new List<SyllabusResponseV2>();
            foreach (var syl in syllabuses)
            {
                var name = "undefined";
                var subjectCode = "undefined";
                var syllabusName = "undefined";
                if (syl.Course != null)
                {
                    name = syl.Course.Name;
                }
                if (syl.SubjectCode != null)
                {
                    subjectCode = syl.SubjectCode;
                }
                if (syl.Name != null)
                {
                    syllabusName = syl.Name;
                }
                if (syl.SubjectCode != null) { }
                SyllabusResponseV2 syllabusResponseV2 = new SyllabusResponseV2
                {
                    Id = syl.Id,
                    CourseName = name,
                    EffectiveDate = syl.EffectiveDate,
                    SubjectCode = subjectCode,
                    SyllabusName = syllabusName,
                    UpdateTime = syl.UpdateTime,
                };
                responses.Add(syllabusResponseV2);
            }
            if (keyword != null)
            {
                responses = (responses.Where(x => (x.SyllabusName.ToLower().Trim().Contains(keyword.ToLower().Trim()) || x.SubjectCode.ToLower().Trim().Contains(keyword.ToLower().Trim())))).ToList();
            }
            if (responses != null)
            {
                responses = (responses.OrderByDescending(x => x.UpdateTime)).ToList();
            }
            return responses;
        }

        public async Task<bool> UpdateSyllabus(OverallSyllabusRequest request, string id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id));
            if (syllabus.CourseId == null)
            {
                await UpdateGeneralSyllabus(syllabus.Id, request);
                await UpdateMaterial(request, syllabus.Id);
                await UpdateExam(request, syllabus.Id);
                await UpdateLearningItems(request, syllabus.Id);
                return await _unitOfWork.CommitAsync() > 0;
            }
            bool isSuccess = await AddSyllabus(request);
            return isSuccess;
        }
        public async Task<bool> UpdateOverallSyllabus(string id, UpdateOverallSyllabus request)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id.ToString()));
            if (syllabus != null)
            {
                if (request.EffectiveDate != null)
                {
                    string format = "dd/MM/yyyy";

                    var date = DateTime.TryParseExact(request.EffectiveDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                        ? (DateTime?)parsedDate
                        : DateTime.Parse(request.EffectiveDate);

                    syllabus.EffectiveDate = date;

                }
                syllabus.UpdateTime = DateTime.Now;
                if (request.Description != null)
                {
                    syllabus.Description = request.Description;
                }
                if (request.MinAvgMarkToPass != null)
                {
                    syllabus.MinAvgMarkToPass = request.MinAvgMarkToPass.Value;
                }
                if (request.SyllabusLink != null)
                {
                    syllabus.SyllabusLink = request.SyllabusLink;
                }
                if (request.SyllabusName != null)
                {
                    syllabus.Name = request.SyllabusName;
                }
                if (request.ScoringScale != null)
                {
                    syllabus.ScoringScale = request.ScoringScale.Value;
                }
                if (request.SubjectCode != null)
                {
                    syllabus.SubjectCode = request.SubjectCode;
                }
                if (request.TimePerSession != null)
                {
                    syllabus.TimePerSession = request.TimePerSession.Value;
                }
                if (request.StudentTasks != null)
                {
                    syllabus.StudentTasks = request.StudentTasks;
                }
                if (request.Type != null)
                {
                    var categoryId = await _unitOfWork.GetRepository<SyllabusCategory>()
          .SingleOrDefaultAsync(selector: x => x.Id, predicate: x => x.Name!.ToLower().Trim().Equals(request.Type!.ToLower().Trim()));
                    syllabus.SyllabusCategoryId = categoryId;
                }
                _unitOfWork.GetRepository<Syllabus>().UpdateAsync(syllabus);
                var isSucess = await _unitOfWork.CommitAsync() > 0;
                return isSucess;
            }
            return false;
        }

        public async Task<bool> UpdateTopic(string id, UpdateTopicRequest request)
        {
            var topic = await _unitOfWork.GetRepository<Topic>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id));
            if (topic == null) { return false; }
            if (request != null)
            {
                if (!request.TopicName.IsNullOrEmpty())
                {
                    topic.Name = request.TopicName;
                }
                _unitOfWork.GetRepository<Topic>().UpdateAsync(topic);
                bool isSuc = await _unitOfWork.CommitAsync() > 0;
            }
            return false;
        }

        public async Task<bool> UpdateSession(string id, UpdateSessionRequest request)
        {
            var sessionDescription = await _unitOfWork.GetRepository<SessionDescription>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id));
            if (sessionDescription == null) { return false; }
            if (request != null)
            {
                if (!request.Content.IsNullOrEmpty())
                {
                    sessionDescription.Content = request.Content;
                }
                if (!request.Content.IsNullOrEmpty())
                {
                    sessionDescription.Detail = request.Detail;
                }
                _unitOfWork.GetRepository<SessionDescription>().UpdateAsync(sessionDescription);
                bool isSuc = await _unitOfWork.CommitAsync() > 0;
                return isSuc;
            }
            return false;
        }
        private async Task UpdateGeneralSyllabus(Guid syllabusId, OverallSyllabusRequest request)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(syllabusId.ToString()));
            if (syllabus != null)
            {
                if (request.EffectiveDate != null)
                {
                    string format = "dd/MM/yyyy";

                    var date = DateTime.TryParseExact(request.EffectiveDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                        ? (DateTime?)parsedDate
                        : DateTime.Parse(request.EffectiveDate);

                    syllabus.EffectiveDate = date;

                }
                syllabus.UpdateTime = DateTime.Now;
                if (!request.Description.IsNullOrEmpty())
                {
                    syllabus.Description = request.Description;
                }
                if (request.MinAvgMarkToPass != null)
                {
                    syllabus.MinAvgMarkToPass = request.MinAvgMarkToPass;
                }
                if (!request.SyllabusLink.IsNullOrEmpty())
                {
                    syllabus.SyllabusLink = request.SyllabusLink;
                }
                if (!request.SyllabusName.IsNullOrEmpty())
                {
                    syllabus.Name = request.SyllabusName;
                }
                if (request.ScoringScale != null)
                {
                    syllabus.ScoringScale = request.ScoringScale;
                }
                if (request.SubjectCode != null)
                {
                    syllabus.SubjectCode = request.SubjectCode;
                }
                if (syllabus.TimePerSession != null)
                {
                    syllabus.TimePerSession = request.TimePerSession;
                }
                if (!request.StudentTasks.IsNullOrEmpty())
                {
                    syllabus.StudentTasks = request.StudentTasks;
                }
                var categoryId = await _unitOfWork.GetRepository<SyllabusCategory>()
             .SingleOrDefaultAsync(selector: x => x.Id, predicate: x => x.Name!.ToLower().Trim().Equals(request.Type!.ToLower().Trim()));
                syllabus.SyllabusCategoryId = categoryId;
                _unitOfWork.GetRepository<Syllabus>().UpdateAsync(syllabus);
                await _unitOfWork.CommitAsync();
            }
        }
        private async Task UpdateMaterial(OverallSyllabusRequest request, Guid syllabusId)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(syllabusId.ToString()));
            var listMaterial = await _unitOfWork.GetRepository<Material>().GetListAsync(predicate: x => x.SyllabusId.ToString().Equals(syllabusId.ToString()));
            _unitOfWork.GetRepository<Material>().DeleteRangeAsync(listMaterial);
            await _unitOfWork.CommitAsync();
            syllabus.Materials = request.MaterialRequests.Select(mat => new Material
            {
                Id = new Guid(),
                SyllabusId = syllabus.Id,
                URL = mat.URL,
                Name = mat.FileName,
            }).ToList();

            await _unitOfWork.GetRepository<Material>().InsertRangeAsync(syllabus.Materials);
        }
        private async Task UpdateExam(OverallSyllabusRequest request, Guid syllabusId)
        {
            var listExam = await _unitOfWork.GetRepository<ExamSyllabus>().GetListAsync(predicate: x => x.SyllabusId.ToString().Equals(syllabusId.ToString()));

            _unitOfWork.GetRepository<ExamSyllabus>().DeleteRangeAsync(listExam);
            await _unitOfWork.CommitAsync();
            await GenerateExam(request, syllabusId);
        }
        private async Task UpdateLearningItems(OverallSyllabusRequest request, Guid syllabusId)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(syllabusId.ToString()));
            var topics = await _unitOfWork.GetRepository<Topic>().GetListAsync(predicate: x => x.SyllabusId.ToString().Equals(syllabus.Id.ToString()));
            List<Session> sessions = new List<Session>();
            foreach (var item in topics)
            {
                var session = await _unitOfWork.GetRepository<Session>().GetListAsync(predicate: x => x.TopicId.ToString().Equals(item.Id.ToString()), include: x => x.Include(x => x.QuestionPackage));
                sessions.AddRange(session);
            }
            List<SessionDescription> sessionDescriptions = new List<SessionDescription>();
            List<QuestionPackage> questionPackages = new List<QuestionPackage>();
            var qpx = await _unitOfWork.GetRepository<QuestionPackage>().GetListAsync();
            foreach (var sessionx in sessions)
            {
                var sessionDescription = await _unitOfWork.GetRepository<SessionDescription>().GetListAsync(predicate: x => x.SessionId.ToString().Equals(sessionx.Id.ToString()));
                sessionDescriptions.AddRange(sessionDescription);
            }
            foreach (var sessionx in sessions)
            {
                var qp = qpx.SingleOrDefault(x => x.SessionId.ToString().Equals(sessionx.Id.ToString()));
                if (qp != null)
                {
                    questionPackages.Add(qp);
                }
            }
            List<Question> questions = new List<Question>();
            foreach (var questionpack in questionPackages)
            {
                var question = await _unitOfWork.GetRepository<Question>().GetListAsync(predicate: x => x.QuestionPacketId.ToString().Equals(questionpack.Id.ToString()), include: x => x.Include(x => x.FlashCards).Include(x => x.MutipleChoices));
                questions.AddRange(question);
            }
            List<FlashCard> flashCards = new List<FlashCard>();
            List<MultipleChoice> mutipleChoiceAnswers = new List<MultipleChoice>();
            foreach (var question in questions)
            {
                if (question.FlashCards.Count > 0)
                {
                    flashCards.AddRange(question.FlashCards);
                }
                if (question.MutipleChoices.Count > 0)
                {
                    mutipleChoiceAnswers.AddRange(question.MutipleChoices);
                }
            }
            List<SideFlashCard> sideFlashCards = new List<SideFlashCard>();
            foreach (var flashCard in flashCards)
            {
                var sideFlashCard = await _unitOfWork.GetRepository<SideFlashCard>().GetListAsync(predicate: x => x.FlashCardId.ToString().Equals(flashCard.Id.ToString()));
                sideFlashCards.AddRange(sideFlashCard);
            }
            _unitOfWork.GetRepository<SideFlashCard>().DeleteRangeAsync(sideFlashCards);
            _unitOfWork.GetRepository<FlashCard>().DeleteRangeAsync(flashCards);
            _unitOfWork.GetRepository<MultipleChoice>().DeleteRangeAsync(mutipleChoiceAnswers);
            _unitOfWork.GetRepository<Question>().DeleteRangeAsync(questions);
            _unitOfWork.GetRepository<QuestionPackage>().DeleteRangeAsync(questionPackages);
            _unitOfWork.GetRepository<SessionDescription>().DeleteRangeAsync(sessionDescriptions);
            _unitOfWork.GetRepository<Session>().DeleteRangeAsync(sessions);
            _unitOfWork.GetRepository<Topic>().DeleteRangeAsync(topics);
            await _unitOfWork.CommitAsync();
            var sessionInsert = await GenerateLearningItems(request.SyllabusRequests, syllabusId);
            await GenerateExerciseItems(request.QuestionPackageRequests!, sessionInsert);
        }

        public async Task<StaffSyllabusResponse> GetStaffSyllabusResponse(string id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id));
            if (syllabus == null)
            {
                return new StaffSyllabusResponse();
            }
            var cagegory = await _unitOfWork.GetRepository<SyllabusCategory>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(syllabus.SyllabusCategoryId.ToString()), selector: x => x.Name);
            List<string> strings = new List<string>();
            var namePre = await _unitOfWork.GetRepository<SyllabusPrerequisite>().GetListAsync(predicate: x => x.CurrentSyllabusId.ToString().Equals(syllabus.Id.ToString()), selector: x => x.PrerequisiteSyllabusId);
            if (namePre != null)
            {
                foreach (var prerequisite in namePre)
                {
                    strings.Add(await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(prerequisite.ToString()), selector: x => x.SubjectCode));
                }
            }
            var count = 0;
            if (syllabus.NumOfSessions != null)
            {
                count = syllabus.NumOfSessions.Value;
            }
            var syllRes = new StaffSyllabusResponse()
            {
                SyllabusLink = syllabus.SyllabusLink,
                Description = syllabus.Description,
                Category = cagegory,
                EffectiveDate = syllabus.EffectiveDate.Value.ToString("dd/MM/yyyy"),
                MinAvgMarkToPass = syllabus.MinAvgMarkToPass,
                ScoringScale = syllabus.ScoringScale,
                StudentTasks = syllabus.StudentTasks,
                SyllabusName = syllabus.Name,
                TimePerSession = syllabus.TimePerSession,
                SubjectCode = syllabus.SubjectCode,
                SyllabusId = syllabus.Id,

            };
            if (strings.Count > 0)
            {
                syllRes.PreRequisite = strings;
            }
            var courseId = syllabus.CourseId;
            if (courseId == null)
            {
                syllRes.LinkedCourse = null;
            }
            else
            {
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseId.ToString()));
                var courseName = course.Name;
                syllRes.LinkedCourse = new PayLoad.Response.Courses.LinkedCourse
                {
                    CourseId = courseId.Value,
                    CourseName = courseName
                };
            }
            syllRes.Materials = await GetMaterialResponse(id);
            syllRes.Exams = await GetStaffExamSyllabusResponses(id);
            syllRes.SessionResponses = await GetAllSessionResponses(id);
            syllRes.QuestionPackages = await GetStaffQuestionPackageResponses(id);
            return syllRes;
        }
        public async Task<List<StaffMaterialResponse>> GetMaterialResponse(string id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id));
            var materials = await _unitOfWork.GetRepository<Material>().GetListAsync(predicate: x => x.SyllabusId.ToString().Equals(syllabus.Id.ToString()));
            if (materials == null)
            {
                return new List<StaffMaterialResponse>();
            }
            List<StaffMaterialResponse> result = new List<StaffMaterialResponse>();
            foreach (var material in materials)
            {
                result.Add(new StaffMaterialResponse()
                {
                    MaterialId = material.Id,
                    Url = material.URL,
                    FileName = material.Name,
                });
            }
            return result;
        }
        public async Task<List<StaffExamSyllabusResponse>> GetStaffExamSyllabusResponses(string id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id));
            var examSyllabuses = await _unitOfWork.GetRepository<ExamSyllabus>().GetListAsync(predicate: x => x.SyllabusId.ToString().Equals(syllabus.Id.ToString()));
            if (examSyllabuses == null)
            {
                return new List<StaffExamSyllabusResponse>();
            }
            List<StaffExamSyllabusResponse> result = new List<StaffExamSyllabusResponse>();
            foreach (var syll in examSyllabuses)
            {
                StaffExamSyllabusResponse staffExamSyllabusResponse = new StaffExamSyllabusResponse
                {
                    ExamSyllabusId = syll.Id,
                    CompletionCriteria = syll.CompleteionCriteria,
                    ContentName = syll.ContentName,
                    Method = syll.Method,
                    Part = syll.Part,
                    QuestionType = syll.QuestionType,
                    Type = syll.Category,
                    Weight = syll.Weight,
                    Duration = syll.Duration,
                };
                result.Add(staffExamSyllabusResponse);
            }
            return result;
        }
        public async Task<List<StaffSessionResponse>> GetAllSessionResponses(string id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id));
            var topics = await _unitOfWork.GetRepository<Topic>().GetListAsync(predicate: x => x.SyllabusId.ToString().Equals(syllabus.Id.ToString()));
            if (topics == null)
            {
                return new List<StaffSessionResponse>();
            }
            List<StaffSessionResponse> sessionResponses = new List<StaffSessionResponse>();
            foreach (var topic in topics)
            {
                sessionResponses.AddRange(await GetStaffSession(topic.Id.ToString()));
            }
            sessionResponses = sessionResponses.OrderBy(x => x.OrderSession).ToList();
            return sessionResponses;
        }
        private async Task<List<StaffSessionResponse>> GetStaffSession(string topicid)
        {
            var sessions = await _unitOfWork.GetRepository<Session>().GetListAsync(predicate: x => x.TopicId.ToString().Equals(topicid), include: x => x.Include(x => x.Topic));
            if (sessions == null)
            {
                return new List<StaffSessionResponse>();
            }
            List<StaffSessionResponse> staffSessionResponses = new List<StaffSessionResponse>();
            foreach (var session in sessions)
            {
                StaffSessionResponse st = new StaffSessionResponse
                {
                    OrderSession = session.NoSession,
                    SessionId = session.Id,
                    OrderTopic = session.Topic.OrderNumber,
                    TopicName = session.Topic.Name,
                };
                st.Contents = await GetStaffSessionDescriptions(session.Id.ToString());
                var qp = await GetPackageQuestionBySessionId(session.Id.ToString());
                if (qp != null)
                {
                    st.StaffQuestionPackageResponse = qp;
                }
                staffSessionResponses.Add(st);
            }
            staffSessionResponses = staffSessionResponses.OrderBy(x => x.OrderSession).ToList();
            return staffSessionResponses;
        }
        private async Task<List<StaffSessionDescriptionResponse>> GetStaffSessionDescriptions(string sessionId)
        {
            var sessionDescriptions = await _unitOfWork.GetRepository<SessionDescription>().GetListAsync(predicate: x => x.SessionId.ToString().Equals(sessionId));
            if (sessionDescriptions == null)
            {
                return new List<StaffSessionDescriptionResponse>();
            }
            List<StaffSessionDescriptionResponse> sessionDescriptionResponses = new List<StaffSessionDescriptionResponse>();
            foreach (var session in sessionDescriptions)
            {
                var des = session.Detail;
                List<string> strings = new List<string>();
                if (des != null)
                {
                    string[] depart = des.Split(new string[] { "/r/n" }, StringSplitOptions.None);
                    for (int i = 0; i < depart.Length; i++)
                    {
                        strings.Add(depart[i]);
                    }
                }
                else
                {
                    strings.Add(string.Empty);
                }
                StaffSessionDescriptionResponse staffSessionDescriptionResponse = new StaffSessionDescriptionResponse
                {
                    Content = session.Content,
                    Details = strings,
                };
                sessionDescriptionResponses.Add(staffSessionDescriptionResponse);
            }
            return sessionDescriptionResponses;
        }
        private async Task<StaffQuestionPackageResponse> GetPackageQuestionBySessionId(string sessionId)
        {
            var questionpackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId.ToString().Equals(sessionId), include: x => x.Include(x => x.Session));
            if (questionpackage == null)
            {
                return null;
            }
            var session = await _unitOfWork.GetRepository<Session>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(questionpackage.SessionId.ToString()));
            return new StaffQuestionPackageResponse
            {
                NoOfSession = session.NoSession,
                QuestionPackageId = questionpackage.Id,
                Title = questionpackage.Title,
                Type = questionpackage.Type,
                Deadline = questionpackage.DeadlineTime,
                Duration = questionpackage.Duration,
                Score = questionpackage.Score.Value,
                //AttemptsAllowed = questionpackage.
                ContentName = questionpackage.ContentName,
            };
        }
        public async Task<List<StaffQuestionPackageResponse>> GetStaffQuestionPackageResponses(string sylId)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(sylId), include: x => x.Include(x => x.Topics));
            if (syllabus.Topics.Count() > 0 && syllabus.Topics != null)
            {
                var topics = syllabus.Topics;
                List<Session> sessions = new List<Session>();
                foreach (var topic in topics)
                {
                    var session = await _unitOfWork.GetRepository<Session>().GetListAsync(predicate: x => x.TopicId.ToString().Equals(topic.Id.ToString()));
                    if (session.Count() > 0 && session != null)
                    {
                        sessions.AddRange(session);
                    }
                }
                List<StaffQuestionPackageResponse> questionPackageResponses = new List<StaffQuestionPackageResponse>();
                foreach (var session in sessions)
                {

                    var qp = await GetPackageQuestionBySessionId(session.Id.ToString());
                    if (qp != null)
                    {
                        questionPackageResponses.Add(qp);
                    }
                }
                return questionPackageResponses;
            }
            return new List<StaffQuestionPackageResponse>();
        }

        public async Task<List<StaffQuestionResponse>> GetStaffQuestions(string questionpackageId)
        {
            var questions = await _unitOfWork.GetRepository<Question>().GetListAsync(predicate: x => x.QuestionPacketId.ToString().Equals(questionpackageId));
            if (questions.Count() == null)
            {
                return new List<StaffQuestionResponse>();
            }
            List<StaffQuestionResponse> questionQuestions = new List<StaffQuestionResponse>();
            foreach (var question in questions)
            {
                var questionQuestion = new StaffQuestionResponse
                {
                    QuestionId = question.Id,
                    Description = question.Description,
                    QuestionImg = question.Img,
                };
                questionQuestion.StaffAnswerResponse = await GetAnswerResponse(question.Id.ToString());
                questionQuestions.Add(questionQuestion);
            }
            return questionQuestions;
        }
        private async Task<StaffAnswerResponse> GetAnswerResponse(string questionId)
        {
            var multiples = await _unitOfWork.GetRepository<MultipleChoice>().GetListAsync(predicate: x => x.QuestionId.ToString().Equals(questionId));
            var flashcards = await _unitOfWork.GetRepository<FlashCard>().GetListAsync(predicate: x => x.QuestionId.ToString().Equals(questionId));
            StaffAnswerResponse response = new StaffAnswerResponse();
            List<StaffMultipleChoiceResponse> multipleChoiceResponses = new List<StaffMultipleChoiceResponse>();
            List<FlashCardAnswerResponseDefault> flashCardAnswerResponses = new List<FlashCardAnswerResponseDefault>();
            if (multiples != null && multiples.Count > 0)
            {
                foreach (var mul in multiples)
                {
                    StaffMultipleChoiceResponse res = new StaffMultipleChoiceResponse
                    {
                        Answer = mul.Description,
                        AnswerImage = mul.Img,
                        MultipleChoiceId = mul.Id,
                        Score = mul.Score,
                    };
                    multipleChoiceResponses.Add(res);
                }
                response.StaffMultiplechoiceAnswerResponses = multipleChoiceResponses;
            }
            if (flashcards != null && flashcards.Count > 0)
            {
                foreach (var flashcard in flashcards)
                {
                    FlashCardAnswerResponseDefault flashCardAnswerResponse = new FlashCardAnswerResponseDefault
                    {
                        FlashCarId = flashcard.Id,
                        Score = flashcard.Score,
                    };
                    flashCardAnswerResponse.SideFlashCardResponses = await GetSideFlashCard(flashcard.Id.ToString());
                    flashCardAnswerResponses.Add(flashCardAnswerResponse);
                }
                response.FlashCardAnswerResponses = flashCardAnswerResponses;
            }
            return response;
        }
        private async Task<List<SideFlashCardResponse>> GetSideFlashCard(string flashcardId)
        {
            var sides = await _unitOfWork.GetRepository<SideFlashCard>().GetListAsync(predicate: x => x.FlashCardId.ToString().Equals(flashcardId));
            if (sides == null)
            {
                return new List<SideFlashCardResponse>();
            }
            List<SideFlashCardResponse> sideFlashCardResponses = new List<SideFlashCardResponse>();
            foreach (var side in sides)
            {
                sideFlashCardResponses.Add(new SideFlashCardResponse
                {
                    Side = side.Side,
                    SideFlashCardDescription = side.Description,
                    SideFlashCardId = side.Id,
                    SideFlashCardImage = side.Image,
                });
            }
            return sideFlashCardResponses;
        }
        public async Task<List<SyllabusResponseV2>> GetStaffSyllabusCanInsert(string? keyword)
        {
            var allSyllabus = await GetAllSyllabus(keyword);
            if (allSyllabus == null)
            {
                return new List<SyllabusResponseV2>();
            }
            List<SyllabusResponseV2> filterSyllabus = new List<SyllabusResponseV2>();
            foreach (var syl in allSyllabus)
            {
                var ix = syl.CourseName.Trim().ToLower().Equals("undefined");
                if (syl.CourseName.Trim().ToLower().Equals("undefined") && (syl.EffectiveDate < DateTime.Now))
                {
                    filterSyllabus.Add(syl);
                }
            }
            if (filterSyllabus == null)
            {
                return new List<SyllabusResponseV2>();
            }
            if (keyword != null)
            {
                filterSyllabus = (filterSyllabus.Where(x => (x.SyllabusName.ToLower().Trim().Contains(keyword.ToLower().Trim()) || x.SubjectCode.ToLower().Trim().Contains(keyword.ToLower().Trim())))).ToList();
            }
            return filterSyllabus.ToList();
        }

        public async Task<bool> UpdateQuiz(string questionpackageId, UpdateQuestionPackageRequest request)
        {
            var questionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(questionpackageId));
            if (questionPackage == null)
            {
                throw new BadHttpRequestException($"không tìm thấy question package có id {questionpackageId}", StatusCodes.Status400BadRequest);
            }
            if (request.ContentName != null)
            {
                questionPackage.ContentName = request.ContentName;
            }
            if (request.Type != null)
            {
                questionPackage.Type = request.Type;
            }
            if (request.Score != null)
            {
                questionPackage.Score = request.Score;
            }
            if (request.Title != null)
            {
                questionPackage.Title = request.Title;
            }
            _unitOfWork.GetRepository<QuestionPackage>().UpdateAsync(questionPackage);
            await _unitOfWork.CommitAsync();
            List<Question> questions = new List<Question>();

            var questionx = await _unitOfWork.GetRepository<Question>().GetListAsync(predicate: x => x.QuestionPacketId.ToString().Equals(questionPackage.Id.ToString()), include: x => x.Include(x => x.FlashCards).Include(x => x.MutipleChoices));
            questions.AddRange(questionx);
            List<FlashCard> flashCards = new List<FlashCard>();
            List<MultipleChoice> mutipleChoiceAnswers = new List<MultipleChoice>();
            foreach (var question in questions)
            {
                if (question.FlashCards.Count > 0)
                {
                    flashCards.AddRange(question.FlashCards);
                }
                if (question.MutipleChoices.Count > 0)
                {
                    mutipleChoiceAnswers.AddRange(question.MutipleChoices);
                }
            }
            List<SideFlashCard> sideFlashCards = new List<SideFlashCard>();
            foreach (var flashCard in flashCards)
            {
                var sideFlashCard = await _unitOfWork.GetRepository<SideFlashCard>().GetListAsync(predicate: x => x.FlashCardId.ToString().Equals(flashCard.Id.ToString()));
                sideFlashCards.AddRange(sideFlashCard);
            }
            _unitOfWork.GetRepository<SideFlashCard>().DeleteRangeAsync(sideFlashCards);
            _unitOfWork.GetRepository<FlashCard>().DeleteRangeAsync(flashCards);
            _unitOfWork.GetRepository<MultipleChoice>().DeleteRangeAsync(mutipleChoiceAnswers);
            _unitOfWork.GetRepository<Question>().DeleteRangeAsync(questions);
            await _unitOfWork.CommitAsync();
            await GenerateQuestionPackgeItems(questionPackage.Id, request.QuestionRequests);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<GeneralSyllabusResponse> GetGeneralSyllabusResponse(string syllabusId)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(syllabusId));
            if (syllabus == null)
            {
                return new GeneralSyllabusResponse();
            }
            var cagegory = await _unitOfWork.GetRepository<SyllabusCategory>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(syllabus.SyllabusCategoryId.ToString()), selector: x => x.Name);
            List<string> strings = new List<string>();
            var namePre = await _unitOfWork.GetRepository<SyllabusPrerequisite>().GetListAsync(predicate: x => x.CurrentSyllabusId.ToString().Equals(syllabus.Id.ToString()), selector: x => x.PrerequisiteSyllabusId);
            if (namePre != null)
            {
                foreach (var prerequisite in namePre)
                {
                    strings.Add(await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(prerequisite.ToString()), selector: x => x.SubjectCode));
                }
            }
            var syllRes = new GeneralSyllabusResponse()
            {
                SyllabusLink = syllabus.SyllabusLink,
                Description = syllabus.Description,
                Category = cagegory,
                EffectiveDate = syllabus.EffectiveDate.Value.ToString("dd/MM/yyyy"),
                MinAvgMarkToPass = syllabus.MinAvgMarkToPass,
                ScoringScale = syllabus.ScoringScale,
                StudentTasks = syllabus.StudentTasks,
                SyllabusName = syllabus.Name,
                TimePerSession = syllabus.TimePerSession,
                SubjectCode = syllabus.SubjectCode,
                SyllabusId = syllabus.Id,

            };
            if (strings.Count > 0)
            {
                syllRes.PreRequisite = strings;
            }
            var courseId = syllabus.CourseId;
            if (courseId == null)
            {
                syllRes.LinkedCourse = null;
            }
            else
            {
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseId.ToString()));
                var courseName = course.Name;
                syllRes.LinkedCourse = new PayLoad.Response.Courses.LinkedCourse
                {
                    CourseId = courseId.Value,
                    CourseName = courseName
                };
            }
            return syllRes;
        }


        #endregion


    }

}
