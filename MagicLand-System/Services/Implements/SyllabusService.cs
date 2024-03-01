using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Engines;
using System.Globalization;
using System.Xml;
using static MagicLand_System.Constants.ApiEndpointConstant;

namespace MagicLand_System.Services.Implements
{
    public class SyllabusService : BaseService<SyllabusService>, ISyllabusService
    {
        public SyllabusService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<SyllabusService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<bool> AddSyllabus(OverallSyllabusRequest request)
        {
            if (request != null)
            {
                try
                {
                    var syllabus = await GenerateSyllabus(request);

                    await GenerateSyllabusItems(request, syllabus.Id);

                    return await _unitOfWork.CommitAsync() > 0;
                }
                catch (Exception ex)
                {
                    throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message + ex.InnerException}]", StatusCodes.Status400BadRequest);
                }
            }
            return false;
        }

        private async Task<Syllabus> GenerateSyllabus(OverallSyllabusRequest request)
        {
            var syllabusesSubjectCode = await _unitOfWork.GetRepository<Syllabus>().GetListAsync(selector: x => x.SubjectCode);
            var numberversion = await _unitOfWork.GetRepository<Syllabus>().GetListAsync(predicate: x => x.Name.ToLower().Trim().Equals(request.SyllabusName));
            var numberCount = 1;
            if (numberversion != null)
            {
                numberCount = numberversion.Count + 1;
            }
            string newSyllabusCode = request.SubjectCode! + "0" + numberCount;

            if (syllabusesSubjectCode.Any(ssc => StringHelper.TrimStringAndNoSpace(ssc!) == StringHelper.TrimStringAndNoSpace(newSyllabusCode)))
            {
                throw new BadHttpRequestException($"Mã Giáo Trình Đã Tồn Tại", StatusCodes.Status400BadRequest);
            }

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
                SubjectCode = newSyllabusCode,
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
                URL = mat,
            }).ToList();

            await _unitOfWork.GetRepository<Syllabus>().InsertAsync(syllabus);
            await _unitOfWork.GetRepository<Material>().InsertRangeAsync(syllabus.Materials);
            return syllabus;
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

                    foreach (var cont in ses.SessionContentRequests)
                    {
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
                        });
                    }
                }

                await _unitOfWork.GetRepository<Session>().InsertRangeAsync(sessionList);
                await _unitOfWork.GetRepository<SessionDescription>().InsertRangeAsync(sessionDescriptionList);

                return sessionList;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.InnerException}]", StatusCodes.Status400BadRequest);
            }
        }
        private async Task GenerateExerciseItems(List<QuestionPackageRequest> questionPackageRequest, List<Session> sessions)
        {
            foreach (var qp in questionPackageRequest)
            {
                Guid newQuestionPackageId = Guid.NewGuid();
                await GenerateQuestionPackage(sessions, qp, newQuestionPackageId);
                await GenerateQuestionPackgeItems(newQuestionPackageId, qp.QuestionRequests);
            }
        }

        private async Task GenerateQuestionPackage(List<Session> sessions, QuestionPackageRequest qp, Guid newQuestionPackageId)
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
                };

                await _unitOfWork.GetRepository<QuestionPackage>().InsertAsync(questionPackage);
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.InnerException}]", StatusCodes.Status400BadRequest);
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
            var mutipleChoiceAnswers = new List<MutipleChoiceAnswer>();
            if (quest.MutipleChoiceAnswerRequests != null)
            {
                foreach (var answer in quest.MutipleChoiceAnswerRequests)
                {
                    mutipleChoiceAnswers.Add(new MutipleChoiceAnswer
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
                await _unitOfWork.GetRepository<MutipleChoiceAnswer>().InsertRangeAsync(mutipleChoiceAnswers);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task<List<SyllabusWithCourseResponse>> FilterSyllabusAsync(List<string>? keyWords, DateTime? date, double? score)
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

            return syllabuses.Select(syll => _mapper.Map<SyllabusWithCourseResponse>(syll)).ToList();
        }


        public async Task<(SyllabusResponse?, SyllabusWithScheduleResponse?)> LoadSyllabusByCourseIdAsync(Guid courseId, Guid classId)
        {
            var syllabus = await ValidateSyllabus(courseId, true);

            if (classId != default)
            {
                var cls = await ValidateClassForSchedule(courseId, classId);

                return (default, SyllabusCustomMapper.fromSyllabusAndClassToSyllabusWithSheduleResponse(syllabus, cls));
            }

            return (_mapper.Map<SyllabusResponse>(syllabus), default);
        }

        private async Task<Class> ValidateClassForSchedule(Guid courseId, Guid classId)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == classId,
                include: x => x.Include(x => x.Schedules.OrderBy(sch => sch.Date)).ThenInclude(sch => sch.Slot)
               .Include(x => x.Schedules.OrderBy(sch => sch.Date)).ThenInclude(sch => sch.Room)
               .Include(x => x.Course)!);

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
               .Include(x => x.ExamSyllabuses!)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
               .ThenInclude(ses => ses.SessionDescriptions)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.QuestionPackage!));
            }
            else
            {
                syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id == id,
                include: x => x.Include(x => x.Materials)
               .Include(x => x.Course)
               .Include(x => x.SyllabusCategory)
               .Include(x => x.ExamSyllabuses!)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
               .ThenInclude(ses => ses.SessionDescriptions)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.QuestionPackage!));
            }

            return syllabus;
        }

        public async Task<(SyllabusResponse?, SyllabusWithCourseResponse?)> LoadSyllabusByIdAsync(Guid id)
        {
            var syllabus = await ValidateSyllabus(id, false);

            if (syllabus.Course == null)
            {
                return (_mapper.Map<SyllabusResponse>(syllabus), default);
            }

            return (default, _mapper.Map<SyllabusWithCourseResponse>(syllabus));

        }

        public async Task<List<SyllabusWithCourseResponse>> LoadSyllabusesAsync()
        {
            var syllabuses = await FetchAllSyllabus();
            return syllabuses.Select(syll => _mapper.Map<SyllabusWithCourseResponse>(syll)).ToList();
        }

        private async Task<List<Syllabus>> FetchAllSyllabus()
        {
            var syllabuses = await _unitOfWork.GetRepository<Syllabus>().GetListAsync(include: x => x.Include(x => x.Materials)
               .Include(x => x.SyllabusCategory)
               .Include(x => x.ExamSyllabuses!)
               .Include(x => x.Course)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
               .ThenInclude(ses => ses.SessionDescriptions)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.QuestionPackage!));

            foreach (var session in syllabuses.SelectMany(syll => syll.Topics!.SelectMany(tp => tp.Sessions!).ToList()).ToList())
            {
                session.QuestionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id);
            }

            return syllabuses.ToList();
        }
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
                };
                responses.Add(syllabusResponseV2);
            }
            if (keyword != null)
            {
                responses = (responses.Where(x => (x.SyllabusName.ToLower().Trim().Contains(keyword.ToLower().Trim()) || x.SubjectCode.ToLower().Trim().Contains(keyword.ToLower().Trim())))).ToList();
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
                URL = mat,
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
                var question = await _unitOfWork.GetRepository<Question>().GetListAsync(predicate: x => x.QuestionPacketId.ToString().Equals(questionpack.Id.ToString()), include: x => x.Include(x => x.FlashCards).Include(x => x.MutipleChoiceAnswers));
                questions.AddRange(question);
            }
            List<FlashCard> flashCards = new List<FlashCard>();
            List<MutipleChoiceAnswer> mutipleChoiceAnswers = new List<MutipleChoiceAnswer>();
            foreach (var question in questions)
            {
                if (question.FlashCards.Count > 0)
                {
                    flashCards.AddRange(question.FlashCards);
                }
                if (question.MutipleChoiceAnswers.Count > 0)
                {
                    mutipleChoiceAnswers.AddRange(question.MutipleChoiceAnswers);
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
            _unitOfWork.GetRepository<MutipleChoiceAnswer>().DeleteRangeAsync(mutipleChoiceAnswers);
            _unitOfWork.GetRepository<Question>().DeleteRangeAsync(questions);
            _unitOfWork.GetRepository<QuestionPackage>().DeleteRangeAsync(questionPackages);
            _unitOfWork.GetRepository<SessionDescription>().DeleteRangeAsync(sessionDescriptions);
            _unitOfWork.GetRepository<Session>().DeleteRangeAsync(sessions);
            _unitOfWork.GetRepository<Topic>().DeleteRangeAsync(topics);
            await _unitOfWork.CommitAsync();
            var sessionInsert = await GenerateLearningItems(request.SyllabusRequests, syllabusId);
            await GenerateExerciseItems(request.QuestionPackageRequests!, sessionInsert);
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
                include: x => x.Include(x => x.Questions!).ThenInclude(quest => quest.MutipleChoiceAnswers!)
                .Include(x => x.Questions!).ThenInclude(quest => quest.FlashCards!).ThenInclude(fc => fc.SideFlashCards!));

            if (questionPackage == null)
            {
                return;
            }

            var exam = course.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => StringHelper.TrimStringAndNoSpace(exam.ContentName!) == StringHelper.TrimStringAndNoSpace(questionPackage.ContentName!));
            var quizResponse = QuizCustomMapper.fromSyllabusItemsToQuizWithQuestionResponse(session.NoSession, questionPackage, exam!);
            quizResponse.SessionId = session.Id;
            quizResponse.CourseId = course.Id;
            quizResponse.Date = "Cần Truy Suất Qua Lớp";

            quizzesResponse.Add(quizResponse);

            //GenerateQuizMutipleChoice(quizzesResponse, course, session, questionPackage);

            //GenerateQuizFlashCard(quizFlashCards, course, session, questionPackage);
        }

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

        private async Task<Class> ValidateClass(Guid id)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == id, include: x => x.Include(x => x.Course).ThenInclude(c => c!.Syllabus)
                .ThenInclude(syll => syll!.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))!
                .Include(x => x.Course).ThenInclude(c => c!.Syllabus).ThenInclude(syll => syll!.ExamSyllabuses!)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)));

            if (cls == null || cls.Course!.Syllabus == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Lớp Học Không Tồn Tại Hoặc Thuộc Khóa Học Không Thuộc Về Bất Cứ Giáo Trình Nào", StatusCodes.Status400BadRequest);
            }

            if (cls.Status == ClassStatusEnum.CANCELED.ToString())
            {
                throw new BadHttpRequestException($"Id [{id}] Của Lớp Học Đã Hủy Không Thể Truy Suất", StatusCodes.Status400BadRequest);
            }

            return cls;
        }

        private async Task GenerateExamWithDate(List<ExamResponse> examsResponse, Class cls, Session session)
        {
            var questionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id,
            include: x => x.Include(x => x.Questions!).ThenInclude(quest => quest.MutipleChoiceAnswers!)
           .Include(x => x.Questions!).ThenInclude(quest => quest.FlashCards!).ThenInclude(fc => fc.SideFlashCards!));

            if (questionPackage == null)
            {
                return;
            }

            var exam = cls.Course!.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => StringHelper.TrimStringAndNoSpace(exam.ContentName!) == StringHelper.TrimStringAndNoSpace(questionPackage.ContentName!));
            var examResponse = QuizCustomMapper.fromSyllabusItemsToExamResponse(session.NoSession, questionPackage, exam!);

            examResponse.SessionId = session.Id;
            examResponse.CourseId = cls.Course.Id;
            examResponse.Date = cls.Schedules.ToList()[session.NoSession - 1].Date.ToString();

            examsResponse.Add(examResponse);
        }

        public async Task<List<ExamResponse>> LoadExamOfClassByClassIdAsync(Guid id)
        {
            var examsResponse = new List<ExamResponse>();

            var cls = await ValidateClass(id);

            foreach (var session in cls.Course!.Syllabus!.Topics!.SelectMany(tp => tp.Sessions!).ToList())
            {
                await GenerateExamWithDate(examsResponse, cls, session);
            }

            return examsResponse;
        }

        public async Task<List<QuizResponse>> LoadQuizOfExamByExamIdAsync(Guid id, int examPart)
        {
            var exam = await _unitOfWork.GetRepository<ExamSyllabus>().SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x.Include(x => x.Syllabus!));
            if (exam == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Bài Kiểm Tra Không Tồn Tại", StatusCodes.Status400BadRequest);
            }
            if (exam.Method == "Offline")
            {
                return default!;
            }

            var sessions = (await _unitOfWork.GetRepository<Syllabus>().GetListAsync(predicate: x => x.Id == exam.SyllabusId, include: x => x.Include(x => x.Topics!)
            .ThenInclude(tp => tp.Sessions!))).SelectMany(syll => syll.Topics!).SelectMany(tp => tp.Sessions!).ToList();

            var questionPackage = new QuestionPackage();
            foreach (var ses in sessions)
            {
                var qp = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == ses.Id,
                    include: x => x.Include(x => x.Questions!).ThenInclude(quest => quest.MutipleChoiceAnswers!)
                   .Include(x => x.Questions!).ThenInclude(quest => quest.FlashCards!).ThenInclude(fc => fc.SideFlashCards));

                if (qp != null && StringHelper.TrimStringAndNoSpace(exam.ContentName!) == StringHelper.TrimStringAndNoSpace(qp.ContentName!))
                {
                    int part = 1;
                    if (qp.Type == "flashcard")
                    {
                        part = 2;
                    }
                    if (examPart == part)
                    {
                        questionPackage = qp;
                    }
                }
            }

            return QuestionCustomMapper.fromQuestionPackageToQuizResponseInLimitScore(questionPackage);
        }
        public async Task<StaffSyllabusResponse> GetStaffSyllabusResponse(string id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id));
            if (syllabus == null)
            {
                return new StaffSyllabusResponse();
            }
            var cagegory = await _unitOfWork.GetRepository<SyllabusCategory>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(syllabus.SyllabusCategoryId.ToString()), selector: x => x.Name);
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
        private async Task<List<StaffMaterialResponse>> GetMaterialResponse(string id)
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
                });
            }
            return result;
        }
        private async Task<List<StaffExamSyllabusResponse>> GetStaffExamSyllabusResponses(string id)
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
        private async Task<List<StaffSessionResponse>> GetAllSessionResponses(string id)
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
                AttemptsAllowed = questionpackage.AttemptsAllowed,
                ContentName = questionpackage.ContentName,
            };
        }
        private async Task<List<StaffQuestionPackageResponse>> GetStaffQuestionPackageResponses(string sylId)
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
            var multiples = await _unitOfWork.GetRepository<MutipleChoiceAnswer>().GetListAsync(predicate: x => x.QuestionId.ToString().Equals(questionId));
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
            foreach(var syl in allSyllabus)
            {
                var ix = syl.CourseName.Trim().ToLower().Equals("undefined");
                if(syl.CourseName.Trim().ToLower().Equals("undefined")  && (syl.EffectiveDate > DateTime.Now))
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
            if(request.ContentName != null)
            {
                questionPackage.ContentName = request.ContentName;
            }
            if(request.Type != null)
            {
                questionPackage.Type = request.Type;
            }
            if(request.Score != null)
            {
                questionPackage.Score = request.Score;
            }
            if(request.Title != null)
            {
                questionPackage.Title = request.Title;
            }
            _unitOfWork.GetRepository<QuestionPackage>().UpdateAsync(questionPackage);
            await _unitOfWork.CommitAsync();
            List<Question> questions = new List<Question>();

            var questionx = await _unitOfWork.GetRepository<Question>().GetListAsync(predicate: x => x.QuestionPacketId.ToString().Equals(questionPackage.Id.ToString()), include: x => x.Include(x => x.FlashCards).Include(x => x.MutipleChoiceAnswers));
            questions.AddRange(questionx);
            List<FlashCard> flashCards = new List<FlashCard>();
            List<MutipleChoiceAnswer> mutipleChoiceAnswers = new List<MutipleChoiceAnswer>();
            foreach (var question in questions)
            {
                if (question.FlashCards.Count > 0)
                {
                    flashCards.AddRange(question.FlashCards);
                }
                if (question.MutipleChoiceAnswers.Count > 0)
                {
                    mutipleChoiceAnswers.AddRange(question.MutipleChoiceAnswers);
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
            _unitOfWork.GetRepository<MutipleChoiceAnswer>().DeleteRangeAsync(mutipleChoiceAnswers);
            _unitOfWork.GetRepository<Question>().DeleteRangeAsync(questions);
            await _unitOfWork.CommitAsync();
            await GenerateQuestionPackgeItems(questionPackage.Id, request.QuestionRequests);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }


}
