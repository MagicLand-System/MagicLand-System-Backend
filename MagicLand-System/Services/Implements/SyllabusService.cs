using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
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
                    throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.InnerException}]", StatusCodes.Status400BadRequest);
                }
            }
            return false;
        }

        private async Task<Syllabus> GenerateSyllabus(OverallSyllabusRequest request)
        {
            Guid newSyllabusId = Guid.NewGuid();
            var syllabus = new Syllabus
            {
                Id = newSyllabusId,
                EffectiveDate = DateTime.Parse(request.EffectiveDate!),
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
                CompleteionCriteria = exam.CompleteionCriteria,
                SyllabusId = syllabusId,
                Duration = exam.Duration,
                QuestionType = exam.QuestionType,
                Weight = exam.Weight,
                Part = exam.Part,
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
                    Type = qp.Type,
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
                    Description = flash.RightSideDescription,
                    Image = flash.RightSideImg,
                    Side = "Left",
                    FlashCardId = newFlashCardId,
                });
            }

            if (flashCardList.Any())
            {
                await _unitOfWork.GetRepository<FlashCard>().InsertRangeAsync(flashCardList);
            }
            if (sideFlashCardList.Any())
            {
                await _unitOfWork.GetRepository<SideFlashCard>().InsertRangeAsync(sideFlashCardList);
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

            return syllabuses.Select(syll => _mapper.Map<SyllabusResponse>(syll)).ToList();
        }


        public async Task<SyllabusResponse> LoadSyllabusByCourseIdAsync(Guid id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.CourseId == id,
               include: x => x.Include(x => x.Materials)
              .Include(x => x.SyllabusCategory)
              .Include(x => x.ExamSyllabuses)
              .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
              .ThenInclude(ses => ses.SessionDescriptions)
              .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.QuestionPackage!));

            if (syllabus == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Khóa Học Không Tồn Tại Hoặc Khóa Học Không Thuộc Về Bất Cứ Giáo Trình Nào", StatusCodes.Status400BadRequest);
            };

            foreach (var session in syllabus.Topics!.SelectMany(tp => tp.Sessions!).ToList())
            {
                session.QuestionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id);
            }

            return _mapper.Map<SyllabusResponse>(syllabus);
        }

        public async Task<SyllabusResponse> LoadSyllabusByIdAsync(Guid id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id == id,
                include: x => x.Include(x => x.Materials)
               .Include(x => x.SyllabusCategory)
               .Include(x => x.ExamSyllabuses)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
               .ThenInclude(ses => ses.SessionDescriptions)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.QuestionPackage!));

            if (syllabus == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Giáo Trình Không Tồn Tại", StatusCodes.Status400BadRequest);
            };

            foreach (var session in syllabus.Topics!.SelectMany(tp => tp.Sessions!).ToList())
            {
                session.QuestionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id);
            }

            return _mapper.Map<SyllabusResponse>(syllabus);
        }

        public async Task<List<SyllabusResponse>> LoadSyllabusesAsync()
        {
            var responses = new List<SyllabusResponse>();
            var syllabuses = await FetchAllSyllabus();
            foreach (var syll in syllabuses)
            {
                var response = _mapper.Map<SyllabusResponse>(syll);
                response.Materials = MaterialCustomMapper.fromMaterialsToMaterialResponse(syll.Materials!);
                var a = QuestionPackageCustomMapper.fromTopicsToQuestionPackageResponse(syll.Topics!);
                response.QuestionPackages = a;
                responses.Add(response);
                var s = response;
            }

            return responses;
        }

        private async Task<List<Syllabus>> FetchAllSyllabus()
        {
            var syllabuses = await _unitOfWork.GetRepository<Syllabus>().GetListAsync(include: x => x.Include(x => x.Materials)
               .Include(x => x.SyllabusCategory)
               .Include(x => x.ExamSyllabuses)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
               .ThenInclude(ses => ses.SessionDescriptions)
               .Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)).ThenInclude(ses => ses.QuestionPackage!));

            foreach (var session in syllabuses.SelectMany(syll => syll.Topics!.SelectMany(tp => tp.Sessions!).ToList()).ToList())
            {
                session.QuestionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id);
            }

            return syllabuses.ToList();
        }
        public async Task<List<SyllabusResponseV2>> GetAllSyllabus(string? keyword)
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
    }
}
