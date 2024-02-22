using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MagicLand_System.Constants.ApiEndpointConstant;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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
                foreach (var material in mat)
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
                foreach (var topicx in topics)
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
                            foreach (var d in detailStrings)
                            {
                                stringFinal = stringFinal + "/r/n" + d.ToString();
                            }
                            SessionDescription sessionDescription = new SessionDescription
                            {
                                Id = Guid.NewGuid(),
                                Detail = stringFinal,
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
                foreach (var ques in questionPack)
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
                    foreach (var question in questionRequest)
                    {
                        Question question1 = new Question
                        {
                            Id = Guid.NewGuid(),
                            Description = question.Description,
                            Img = question.Img,
                            QuestionPacketId = qp.Id,
                        };
                        questionList.Add(question1);
                        if (question.MutipleChoiceAnswerRequests != null)
                        {
                            var mul = question.MutipleChoiceAnswerRequests;
                            foreach (var answer in mul)
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
                        if (question.FlashCardRequests != null)
                        {
                            var flashcard = question.FlashCardRequests;
                            foreach (var flash in flashcard)
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
                if (flashCards.Count > 0 && flashCards != null)
                {
                    await _unitOfWork.GetRepository<FlashCard>().InsertRangeAsync(flashCards);
                }
                if (sideFlashCards.Count > 0 && sideFlashCards != null)
                {
                    await _unitOfWork.GetRepository<SideFlashCard>().InsertRangeAsync(sideFlashCards);
                }
                var isSucess = await _unitOfWork.CommitAsync() > 0;
                return isSucess;
            }
            return false;
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

            return syllabuses.Select(syll => _mapper.Map<SyllabusResponse>(syll)).ToList();
        }


        public async Task<SyllabusResponse> LoadSyllabusByCourseIdAsync(Guid id)
        {
            var syllabus = await _unitOfWork.GetRepository<CourseSyllabus>().SingleOrDefaultAsync(predicate: x => x.CourseId == id,
               include: x => x.Include(x => x.Materials)
               .Include(x => x.ExamSyllabuses)
               .Include(x => x.Topics.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.QuestionPackage)!);

            if (syllabus == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Khóa Học Không Tồn Tại Hoặc Khóa Học Không Thuộc Về Bất Cứ Giáo Trình Nào", StatusCodes.Status400BadRequest);
            };

            return _mapper.Map<SyllabusResponse>(syllabus);
        }

        public async Task<SyllabusResponse> LoadSyllabusByIdAsync(Guid id)
        {
            var syllabus = await _unitOfWork.GetRepository<CourseSyllabus>().SingleOrDefaultAsync(predicate: x => x.Id == id,
                include: x => x.Include(x => x.Materials)
                .Include(x => x.ExamSyllabuses)
                .Include(x => x.Topics.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.QuestionPackage)!);

            if (syllabus == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Giáo Trình Không Tồn Tại", StatusCodes.Status400BadRequest);
            };

            return _mapper.Map<SyllabusResponse>(syllabus);
        }

        public async Task<List<SyllabusResponse>> LoadSyllabusesAsync()
        {
            var syllabuses = await FetchAllSyllabus();
            return syllabuses.Select(syll => _mapper.Map<SyllabusResponse>(syll)).ToList();
        }

        private async Task<List<CourseSyllabus>> FetchAllSyllabus()
        {
            var syllabuses = await _unitOfWork.GetRepository<CourseSyllabus>().GetListAsync(include: x => x.Include(x => x.Materials)
                .Include(x => x.ExamSyllabuses)
                .Include(x => x.Topics.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.QuestionPackage)!);

            return syllabuses.ToList();
        }
    }
}
