﻿using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
            var examList = request.ExamSyllabusRequests.Select((exam, index) => new ExamSyllabus
            {
                Category = exam.Type,
                CompleteionCriteria = exam.CompleteionCriteria,
                SyllabusId = syllabusId,
                Duration = exam.Duration,
                QuestionType = exam.QuestionType,
                Weight = exam.Weight,
                Part = exam.Part,
                ExamOrder = index + 1,
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
            int index = 1;
            foreach (var qp in questionPackageRequest)
            {
                Guid newQuestionPackageId = Guid.NewGuid();
                await GenerateQuestionPackage(sessions, qp, newQuestionPackageId, index);
                await GenerateQuestionPackgeItems(newQuestionPackageId, qp.QuestionRequests);
                index++;
            }
        }

        private async Task GenerateQuestionPackage(List<Session> sessions, QuestionPackageRequest qp, Guid newQuestionPackageId, int index)
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
                    PackageOrder = index,
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
                    Image = flash.LeftSideImg,
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
                throw new BadHttpRequestException($"Id [{id}] Của Khóa Học Không Tồn Tại Hoặc Khóa Học Không Thuộc Về Bất Cứ Giáo Trình Nào", StatusCodes.Status400BadRequest);
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
               .Include(x => x.ExamSyllabuses!.OrderBy(exam => exam.ExamOrder))
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
               .Include(x => x.ExamSyllabuses!.OrderBy(exam => exam.ExamOrder))
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
               .Include(x => x.ExamSyllabuses!.OrderBy(exam => exam.ExamOrder))
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

        public async Task<bool> UpdateSyllabus(OverallSyllabusRequest request, string id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync( predicate :  x => x.Id.ToString().Equals(id));
            if(syllabus.CourseId == null) 
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
                if(request.EffectiveDate != null)
                {
                    syllabus.EffectiveDate = DateTime.Parse(request.EffectiveDate);

                }
                syllabus.UpdateTime = DateTime.Now;
                if (!request.Description.IsNullOrEmpty())
                {
                    syllabus.Description = request.Description;
                }
                if(request.MinAvgMarkToPass != null) 
                {
                    syllabus.MinAvgMarkToPass = request.MinAvgMarkToPass;
                }
                if(!request.SyllabusLink.IsNullOrEmpty()) 
                {
                    syllabus.SyllabusLink = request.SyllabusLink;
                }
                if(!request.SyllabusName.IsNullOrEmpty())
                {
                    syllabus.Name = request.SyllabusName;
                }
                if(request.ScoringScale != null)
                {
                    syllabus.ScoringScale = request.ScoringScale;
                }
                if(request.SubjectCode != null)
                {
                    syllabus.SubjectCode = request.SubjectCode; 
                }
                if(syllabus.TimePerSession != null)
                {
                    syllabus.TimePerSession = request.TimePerSession;
                }
                if(!request.StudentTasks.IsNullOrEmpty())
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
        private async Task UpdateMaterial(OverallSyllabusRequest request , Guid syllabusId)
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
        private async Task UpdateExam(OverallSyllabusRequest request , Guid syllabusId)
        {
            var listExam = await _unitOfWork.GetRepository<ExamSyllabus>().GetListAsync(predicate: x => x.SyllabusId.ToString().Equals(syllabusId.ToString()));
          
             _unitOfWork.GetRepository<ExamSyllabus>().DeleteRangeAsync(listExam);
            await _unitOfWork.CommitAsync();
            await GenerateExam(request , syllabusId);
        }
        private async Task UpdateLearningItems(OverallSyllabusRequest request , Guid syllabusId)
        {
             var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(syllabusId.ToString()));
             var topics = await _unitOfWork.GetRepository<Topic>().GetListAsync(predicate : x => x.SyllabusId.ToString().Equals(syllabus.Id.ToString()));
             List<Session> sessions = new List<Session>();
             foreach (var item in topics)
            {
                var session = await _unitOfWork.GetRepository<Session>().GetListAsync(predicate: x => x.TopicId.ToString().Equals(item.Id.ToString()) , include : x => x.Include(x => x.QuestionPackage));
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
            foreach(var sessionx in sessions)
            {
                var qp = qpx.SingleOrDefault(x => x.SessionId.ToString().Equals(sessionx.Id.ToString()));
                if (qp != null)
                {
                    questionPackages.Add(qp);
                }
            }
            List<Question> questions = new List<Question>();    
            foreach(var questionpack in questionPackages)
            {
                var question = await _unitOfWork.GetRepository<Question>().GetListAsync(predicate: x => x.QuestionPacketId.ToString().Equals(questionpack.Id.ToString()) , include : x => x.Include(x => x.FlashCards).Include(x => x.MutipleChoiceAnswers));
                questions.AddRange(question);
            }
            List<FlashCard> flashCards = new List<FlashCard>();
            List <MutipleChoiceAnswer> mutipleChoiceAnswers = new List<MutipleChoiceAnswer>();
            foreach(var question in questions)
            {
                if(question.FlashCards.Count > 0 )
                {
                    flashCards.AddRange(question.FlashCards);
                }
                if(question.MutipleChoiceAnswers.Count > 0)
                {
                    mutipleChoiceAnswers.AddRange(question.MutipleChoiceAnswers);
                }
            }
            List<SideFlashCard> sideFlashCards = new List<SideFlashCard>();
            foreach(var flashCard in flashCards)
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
            await GenerateExerciseItems(request.QuestionPackageRequests!, sessions);
        }


        public async Task<(List<QuizMultipleChoiceResponse>, List<QuizFlashCardResponse>)> LoadQuizzesAsync()
        {
            var quizMultipleChoices = new List<QuizMultipleChoiceResponse>();
            var quizFlashCards = new List<QuizFlashCardResponse>();

            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(
               include: x => x.Include(x => x.Syllabus!)
              .ThenInclude(syll => syll.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
              .Include(x => x.Syllabus).ThenInclude(syll => syll!.ExamSyllabuses!.OrderBy(exam => exam.ExamOrder)));

            await GenerateQuizzesResponse(quizMultipleChoices, quizFlashCards, courses);

            return (quizMultipleChoices, quizFlashCards);
        }

        private async Task GenerateQuizzesResponse(List<QuizMultipleChoiceResponse> quizMultipleChoices, List<QuizFlashCardResponse> quizFlashCards, ICollection<Course> courses)
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
                    await GenerateQuizzes(quizMultipleChoices, quizFlashCards, course, session);
                }
            }
        }

        private async Task GenerateQuizzes(List<QuizMultipleChoiceResponse> quizMultipleChoices, List<QuizFlashCardResponse> quizFlashCards, Course course, Session session)
        {
            var questionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id,
                include: x => x.Include(x => x.Questions!).ThenInclude(quest => quest.MutipleChoiceAnswers!)
                .Include(x => x.Questions!).ThenInclude(quest => quest.FlashCards!).ThenInclude(fc => fc.SideFlashCards!));

            if(questionPackage == null)
            {
                return;
            }

            GenerateQuizMutipleChoice(quizMultipleChoices, course, session, questionPackage);

            GenerateQuizFlashCard(quizFlashCards, course, session, questionPackage);
        }

        private void GenerateQuizFlashCard(List<QuizFlashCardResponse> quizFlashCards, Course course, Session session, QuestionPackage questionPackage)
        {
            if (questionPackage.Questions!.SelectMany(quest => quest.FlashCards!).Any())
            {
                var exam = course.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => exam.ExamOrder == questionPackage.PackageOrder);
                var responseFlashCard = QuizCustomMapper.fromSyllabusItemsToQuizFlashCardResponse(session.NoSession, questionPackage, exam!);
                responseFlashCard.SessionId = session.Id;
                responseFlashCard.CourseId = course.Id;

                quizFlashCards.Add(responseFlashCard);
            }
        }

        private void GenerateQuizMutipleChoice(List<QuizMultipleChoiceResponse> quizMultipleChoices, Course course, Session session, QuestionPackage questionPackage)
        {
            if (questionPackage.Questions!.SelectMany(quest => quest.MutipleChoiceAnswers!).Any())
            {
                var exam = course.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => exam.ExamOrder == questionPackage.PackageOrder);
                var responseMutipleChoice = QuizCustomMapper.fromSyllabusItemsToQuizMutipleChoiceResponse(session.NoSession, questionPackage, exam!);
                responseMutipleChoice.SessionId = session.Id;
                responseMutipleChoice.CourseId = course.Id;

                quizMultipleChoices.Add(responseMutipleChoice);
            }
        }

        public async Task<(List<QuizMultipleChoiceResponse>, List<QuizFlashCardResponse>)> LoadQuizzesByCourseIdAsync(Guid id)
        {
            var quizMultipleChoices = new List<QuizMultipleChoiceResponse>();
            var quizFlashCards = new List<QuizFlashCardResponse>();

            var courses = await _unitOfWork.GetRepository<Course>().GetListAsync(
                predicate: x => x.Id == id,
                include: x => x.Include(x => x.Syllabus!).ThenInclude(syll => syll.Topics!.OrderBy(tp => tp.OrderNumber))
               .ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))
               .Include(x => x.Syllabus).ThenInclude(syll => syll!.ExamSyllabuses!.OrderBy(exam => exam.ExamOrder)));

            if (!courses.Any())
            {
                throw new BadHttpRequestException($"Id [{id}] Của Khóa Học Không Tồn Tại Hoặc Khóa Học Không Thuộc Về Bất Cứ Giáo Trình Nào", StatusCodes.Status400BadRequest);
            }

            await GenerateQuizzesResponse(quizMultipleChoices, quizFlashCards, courses);

            return (quizMultipleChoices, quizFlashCards);
        }

        public async Task<(List<QuizMultipleChoiceResponse>, List<QuizFlashCardResponse>)> LoadQuizzesByClassIdAsync(Guid id)
        {
            var quizMultipleChoices = new List<QuizMultipleChoiceResponse>();
            var quizFlashCards = new List<QuizFlashCardResponse>();

            var cls = await ValidateClass(id);

            foreach (var session in cls.Course!.Syllabus!.Topics!.SelectMany(tp => tp.Sessions!).ToList())
            {
                await GenerateQuizWithDate(quizMultipleChoices, quizFlashCards, cls, session);
            }

            return (quizMultipleChoices, quizFlashCards);
        }

        private async Task<Class> ValidateClass(Guid id)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == id, include: x => x.Include(x => x.Course).ThenInclude(c => c!.Syllabus)
                .ThenInclude(syll => syll!.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))!
                .Include(x => x.Course).ThenInclude(c => c!.Syllabus).ThenInclude(syll => syll!.ExamSyllabuses!.OrderBy(exam => exam.ExamOrder))!
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

        private async Task GenerateQuizWithDate(List<QuizMultipleChoiceResponse> quizMultipleChoices, List<QuizFlashCardResponse> quizFlashCards, Class cls, Session session)
        {
            var questionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id,
            include: x => x.Include(x => x.Questions!).ThenInclude(quest => quest.MutipleChoiceAnswers!)
           .Include(x => x.Questions!).ThenInclude(quest => quest.FlashCards!).ThenInclude(fc => fc.SideFlashCards!));

            if (questionPackage != null && questionPackage.Questions!.SelectMany(quest => quest.MutipleChoiceAnswers!).ToList().Any())
            {
                var exam = cls.Course!.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => exam.ExamOrder == questionPackage.PackageOrder);
                var responseMutipleChoice = QuizCustomMapper.fromSyllabusItemsToQuizMutipleChoiceResponse(session.NoSession, questionPackage, exam!);

                responseMutipleChoice.SessionId = session.Id;
                responseMutipleChoice.CourseId = cls.Course.Id;
                responseMutipleChoice.Date = cls.Schedules.ToList()[session.NoSession - 1].Date.ToString();

                quizMultipleChoices.Add(responseMutipleChoice);
            }

            if (questionPackage != null && questionPackage.Questions!.SelectMany(quest => quest.FlashCards!.Select(fc => fc.SideFlashCards)).ToList().Any())
            {
                var exam = cls.Course!.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => exam.ExamOrder == questionPackage.PackageOrder);
                var responseFlashCard = QuizCustomMapper.fromSyllabusItemsToQuizFlashCardResponse(session.NoSession, questionPackage, exam!);

                responseFlashCard.SessionId = session.Id;
                responseFlashCard.CourseId = cls.Course.Id;
                responseFlashCard.Date = cls.Schedules.ToList()[session.NoSession - 1].Date.ToString();

                quizFlashCards.Add(responseFlashCard);
            }
        }
    }
}
