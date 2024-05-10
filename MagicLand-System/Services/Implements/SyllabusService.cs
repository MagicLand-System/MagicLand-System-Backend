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
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

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
                var overal = await GenerateOveralInfor(request);

                SettingAndChekingQPR(request);

                var syllabusItems = GenerateSyllabusItems(request, overal.Item1.Id);

                try
                {
                    await _unitOfWork.GetRepository<Syllabus>().InsertAsync(overal.Item1);
                    if (overal.Item2.Count > 0)
                    {
                        await _unitOfWork.GetRepository<SyllabusPrerequisite>().InsertRangeAsync(overal.Item2);
                    }

                    await _unitOfWork.GetRepository<Topic>().InsertRangeAsync(syllabusItems.Item1);
                    await _unitOfWork.GetRepository<QuestionPackage>().InsertRangeAsync(syllabusItems.Item2);
                    await _unitOfWork.GetRepository<ExamSyllabus>().InsertRangeAsync(syllabusItems.Item3);

                    return _unitOfWork.Commit() > 0;
                }
                catch (Exception ex)
                {
                    throw new BadHttpRequestException($"{ex.Message + ex.InnerException}", StatusCodes.Status500InternalServerError);
                }
            }
            return false;
        }

        private void SettingAndChekingQPR(OverallSyllabusRequest request)
        {
            double totalWeight = request.ExamSyllabusRequests.Sum(esr => esr.Weight);
            if (totalWeight != 100)
            {
                throw new BadHttpRequestException($"Tổng Trọng Lượng Điểm Số Của Các Bài Kiểm Tra [{totalWeight}] Không Bằng 100", StatusCodes.Status400BadRequest);
            }

            foreach (var qp in request.QuestionPackageRequests)
            {
                if (StringHelper.TrimStringAndNoSpace(qp.Type).ToLower() != QuizTypeEnum.flashcard.ToString() &&
                    StringHelper.TrimStringAndNoSpace(qp.Type).ToLower() != "multiple-choice")
                {
                    throw new BadHttpRequestException($"Loài Bộ Đề [{qp.Type}] Của [{qp.Title}] Không Hợp Lệ", StatusCodes.Status400BadRequest);
                }

            }

            var offlineExams = request.ExamSyllabusRequests
           .Where(exam => exam.Method.Trim().ToLower() == QuizTypeEnum.offline.ToString())
           .ToList();

            var onlineExams = request.ExamSyllabusRequests
           .Where(exam => exam.Method.Trim().ToLower() == QuizTypeEnum.online.ToString()
            && StringHelper.TrimStringAndNoSpace(exam.Type).ToLower() != PackageTypeEnum.Participation.ToString().ToLower())
           .ToList();

            var offlineQuestionPackage = new List<QuestionPackageRequest>();

            if (onlineExams.Any())
            {
                foreach (var exam in onlineExams)
                {
                    var packageFound = request.QuestionPackageRequests.Where(
                        qpr => StringHelper.TrimStringAndNoSpace(qpr.ContentName).ToLower() == StringHelper.TrimStringAndNoSpace(exam.ContentName).ToLower())
                       .ToList();


                    if (packageFound.Any())
                    {
                        if (packageFound.Count != exam.Part)
                        {
                            throw new BadHttpRequestException($"Bài Kiểm Tra [{exam.ContentName}] Không Đủ Số Lượng Bộ Đề Câu Hỏi Và Trả Lời Phù Hợp", StatusCodes.Status400BadRequest);
                        }
                    }
                    else
                    {
                        throw new BadHttpRequestException($"Bài Kiểm Tra [{exam.ContentName}] Không Tìm Thấy Bộ Đề Câu Hỏi Và Trả Lời Phù Hợp", StatusCodes.Status400BadRequest);
                    }

                }
            }

            if (offlineExams.Any())
            {
                var sessionsRequest = request.SyllabusRequests
               .SelectMany(sr => sr.SessionRequests)
               .ToList();

                offlineQuestionPackage.AddRange(
                    from session in sessionsRequest
                    from content in session.SessionContentRequests
                    join exam in offlineExams on content.Content.Trim().ToLower() equals exam.ContentName.Trim().ToLower()
                    select new QuestionPackageRequest
                    {
                        ContentName = exam.ContentName,
                        NoOfSession = session.Order,
                        Type = QuizTypeEnum.offline.ToString(),
                        Title = "Làm Tại Nhà",
                        Score = 0,
                        Duration = 0,
                        QuestionRequests = new List<QuestionRequest>(),
                    });

                if (offlineQuestionPackage.Count == 0 || offlineQuestionPackage.Count != offlineExams.Count)
                {
                    throw new BadHttpRequestException($"Các Bài Kiểm Tra Tại Nhà Không Tìm Thấy Thứ Tự Buổi Học Phù Hợp Hoặc Thứ Tự Các Buổi Học Không Phân Bố Đủ Số Bài Tập", StatusCodes.Status400BadRequest);
                }

                request.QuestionPackageRequests.AddRange(offlineQuestionPackage);
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
        private async Task<(Syllabus, List<SyllabusPrerequisite>)> GenerateOveralInfor(OverallSyllabusRequest request)
        {
            var syllabuses = (await _unitOfWork.GetRepository<Syllabus>().GetListAsync()).ToList();

            ValidateSyllabus(request.SubjectCode, request.SyllabusName, syllabuses);

            Guid newSyllabusId = Guid.NewGuid();

            var syllabus = new Syllabus
            {
                Id = newSyllabusId,
                UpdateTime = DateTime.Now,
                Description = request.Description,
                MinAvgMarkToPass = request.MinAvgMarkToPass!.Value,
                Name = request.SyllabusName,
                ScoringScale = request.ScoringScale!.Value,
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
                    : DateTime.TryParse(request.EffectiveDate, out DateTime normalParedDate)
                    ? normalParedDate
                    : throw new BadHttpRequestException($"Định Dạng Ngày Hiệu Lực Không Hợp Lệ [{request.EffectiveDate}]", StatusCodes.Status400BadRequest);

                syllabus.EffectiveDate = date;

            }

            var syllabusPrerequisties = new List<SyllabusPrerequisite>();
            if (request.PreRequisite != null && request.PreRequisite.Any())
            {
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

                //await _unitOfWork.GetRepository<SyllabusPrerequisite>().InsertRangeAsync(syllabusPrerequisties);
            }

            var categoryId = await _unitOfWork.GetRepository<SyllabusCategory>()
               .SingleOrDefaultAsync(selector: x => x.Id, predicate: x => x.Name!.ToLower().Trim().Equals(request.Type.ToLower().Trim()));

            if (categoryId == default)
            {
                throw new BadHttpRequestException($"Loại Môn Học Không Hợp Lệ [{request.Type}]", StatusCodes.Status400BadRequest);
            }

            syllabus.SyllabusCategoryId = categoryId;

            syllabus.Materials = request.MaterialRequests.Select(mat => new Material
            {
                Id = new Guid(),
                SyllabusId = newSyllabusId,
                URL = mat.URL,
                Name = mat.FileName,
            }).ToList();

            //await _unitOfWork.GetRepository<Syllabus>().InsertAsync(syllabus);
            //await _unitOfWork.GetRepository<Material>().InsertRangeAsync(syllabus.Materials);
            return (syllabus, syllabusPrerequisties);
        }

        private void ValidateSyllabus(string syllabusCode, string syllabusName, List<Syllabus> syllabuses)
        {
            var syllabusesSubjectCode = syllabuses.Select(syll => syll.SubjectCode!.Substring(0, syll.SubjectCode.Length - 2)).ToList();
            if (syllabusesSubjectCode.Any(ssc => StringHelper.TrimStringAndNoSpace(ssc!) == StringHelper.TrimStringAndNoSpace(syllabusCode)))
            {
                throw new BadHttpRequestException($"Mã Giáo Trình [{syllabusCode}] Đã Tồn Tại", StatusCodes.Status400BadRequest);
            }

            var syllabusesName = syllabuses.Select(syll => StringHelper.TrimStringAndNoSpace(syll.Name!)).ToList();
            if (syllabusesName.Any(sn => sn == StringHelper.TrimStringAndNoSpace(syllabusName)))
            {
                throw new BadHttpRequestException($"Tên Giáo Trình [{syllabusName}] Đã Tồn Tại", StatusCodes.Status400BadRequest);
            }
        }

        private (List<Topic>, List<QuestionPackage>, List<ExamSyllabus>) GenerateSyllabusItems(OverallSyllabusRequest request, Guid syllabusId)
        {

            var topics = GenerateLearningItems(request.SyllabusRequests, syllabusId);

            var questionPackages = GenerateExerciseItems(request.QuestionPackageRequests!, request.ExamSyllabusRequests, topics.SelectMany(tp => tp.Sessions!).ToList());

            var exams = GenerateExam(request.ExamSyllabusRequests.ToList(), syllabusId);

            return (topics, questionPackages, exams);
        }

        private List<ExamSyllabus> GenerateExam(List<ExamSyllabusRequest> esr, Guid syllabusId)
        {
            var exams = esr.Select(exam => new ExamSyllabus
            {
                Category = exam.Type,
                ContentName = exam.ContentName,
                CompleteionCriteria = exam.CompleteionCriteria!.Value,
                SyllabusId = syllabusId,
                QuestionType = exam.QuestionType,
                Weight = exam.Weight,
                Part = exam.Part!.Value,
                Method = exam.Method,
                Duration = exam.Duration,
            }).ToList();

            return exams;
        }

        private List<Topic> GenerateLearningItems(List<SyllabusRequest> syllabusRequest, Guid syllabusId)
        {
            var topics = new List<Topic>();

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

                var sessions = GenerateSession(tp.SessionRequests, newTopicId);
                topic.Sessions = sessions;

                topics.Add(topic);
            }

            return topics;
        }

        private List<Session> GenerateSession(List<SessionRequest> sessionRequest, Guid newTopicId)
        {
            try
            {
                var sessions = new List<Session>();

                foreach (var ses in sessionRequest)
                {
                    var sessionDescriptions = new List<SessionDescription>();

                    Guid newSessionId = Guid.NewGuid();
                    var newSession = new Session
                    {
                        Id = newSessionId,
                        TopicId = newTopicId,
                        NoSession = ses.Order,

                    };


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

                        sessionDescriptions.Add(new SessionDescription
                        {
                            Id = Guid.NewGuid(),
                            Detail = stringFinal,
                            SessionId = newSessionId,
                            Content = cont.Content,
                            Order = orderContent,
                        });
                    }

                    newSession.SessionDescriptions = sessionDescriptions;
                    sessions.Add(newSession);
                }

                //await _unitOfWork.GetRepository<Session>().InsertRangeAsync(sessions);
                //await _unitOfWork.GetRepository<SessionDescription>().InsertRangeAsync(sessionDescriptions);

                return sessions;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private List<QuestionPackage> GenerateExerciseItems(List<QuestionPackageRequest> questionPackageRequest, List<ExamSyllabusRequest> examRequest, List<Session> sessions)
        {


            questionPackageRequest = questionPackageRequest.OrderBy(qp => qp.NoOfSession).ToList();
            var questionPackages = new List<QuestionPackage>();

            foreach (var qpr in questionPackageRequest)
            {
                Guid newQuestionPackageId = Guid.NewGuid();
                var examFound = examRequest.SingleOrDefault(er => StringHelper.TrimStringAndNoSpace(er.ContentName).ToLower() == StringHelper.TrimStringAndNoSpace(qpr.ContentName).ToLower());

                var questionPackage = GenerateQuestionPackage(sessions, qpr, examFound, newQuestionPackageId);

                if (qpr.QuestionRequests.Count > 0)
                {
                    var questions = GenerateQuestionPackgeItems(newQuestionPackageId, qpr.QuestionRequests);
                    questionPackage.Questions = questions;
                }

                questionPackages.Add(questionPackage);
            }

            var groupedObjects = questionPackages.GroupBy(qp => qp.PackageType);

            foreach (var gruop in groupedObjects)
            {
                int orderPackageType = 0;
                foreach (var qp in gruop.ToList())
                {
                    orderPackageType++;
                    qp.OrderPackage = orderPackageType;
                }
            }

            return questionPackages;

        }

        private QuestionPackage GenerateQuestionPackage(List<Session> sessions, QuestionPackageRequest qp, ExamSyllabusRequest? exam, Guid newQuestionPackageId)
        {
            var sessionFound = sessions.SingleOrDefault(x => x.NoSession == qp.NoOfSession);
            string packageType = PackageTypeEnum.Review.ToString();

            if (exam != null)
            {
                if (StringHelper.TrimStringAndNoSpace(exam.Type).ToLower() == PackageTypeEnum.Practice.ToString().ToLower())
                {
                    packageType = PackageTypeEnum.Practice.ToString();
                }
                if (StringHelper.TrimStringAndNoSpace(exam.Type).ToLower() == PackageTypeEnum.ProgressTest.ToString().ToLower())
                {
                    packageType = PackageTypeEnum.ProgressTest.ToString();
                }
                if (StringHelper.TrimStringAndNoSpace(exam.Type).ToLower() == PackageTypeEnum.FinalExam.ToString().ToLower())
                {
                    packageType = PackageTypeEnum.FinalExam.ToString();
                }
            }

            if (sessionFound is null)
            {
                throw new BadHttpRequestException($"Không Tìm Thấy Thứ Tự Buổi Học Phù Hợp Cho Bộ Đề [{qp.ContentName}]", StatusCodes.Status400BadRequest);
            }

            return new QuestionPackage
            {
                Id = newQuestionPackageId,
                SessionId = sessionFound!.Id,
                Title = qp.Title,
                ContentName = qp.ContentName,
                Score = qp.Score,
                NoSession = qp.NoOfSession,
                PackageType = packageType,
                QuizType = qp.Type,
            };
        }


        private QuestionPackage GenerateQuestionPackageStaff(List<Session> sessions, QuestionPackageRequest qp, Guid newQuestionPackageId, int orderPackage, int noLastSession)
        {
            try
            {
                var sessionFound = sessions.SingleOrDefault(x => x.NoSession == qp.NoOfSession);
                if (sessionFound is null)
                {
                    throw new BadHttpRequestException($"Không Tìm Thấy Thứ Tự Buổi Học Phù Hợp Cho Bộ Đề [{qp.ContentName}]", StatusCodes.Status400BadRequest);
                }

                return new QuestionPackage
                {
                    Id = newQuestionPackageId,
                    SessionId = sessionFound!.Id,
                    Title = qp.Title,
                    ContentName = qp.ContentName,
                    QuizType = qp.Type,
                    Score = qp.Score,
                    OrderPackage = orderPackage,
                    NoSession = qp.NoOfSession,
                };
            }
            catch (Exception)
            {
                throw;
            }
        }



        private List<Question> GenerateQuestionPackgeItems(Guid newQuestionPackageId, List<QuestionRequest> questionRequest)
        {
            var questions = new List<Question>();

            foreach (var qr in questionRequest)
            {
                Guid newQuestionId = Guid.NewGuid();

                var question = GenerateQuestion(newQuestionPackageId, qr, newQuestionId);

                if (qr.MutipleChoiceAnswerRequests != null && qr.MutipleChoiceAnswerRequests.Count > 0)
                {
                    var mutipleChoices = GenerateMutipleChoice(qr.MutipleChoiceAnswerRequests, newQuestionId);
                    question.MutipleChoices = mutipleChoices;
                }

                if (qr.FlashCardRequests != null && qr.FlashCardRequests.Count > 0)
                {
                    var flasCards = GenerateFlashCardItems(qr.FlashCardRequests, newQuestionId);
                    question.FlashCards = flasCards;
                }

                questions.Add(question);
            }

            return questions;
        }

        private List<FlashCard> GenerateFlashCardItems(List<FlashCardRequest> fcr, Guid newQuestionId)
        {
            var flashCardList = new List<FlashCard>();

            foreach (var fc in fcr)
            {
                Guid newFlashCardId = Guid.NewGuid();
                var sideFlashCardList = new List<SideFlashCard>();

                var newFlashCard = new FlashCard
                {
                    Id = newFlashCardId,
                    QuestionId = newQuestionId,
                    Score = fc.Score,
                };

                sideFlashCardList.Add(new SideFlashCard
                {
                    Description = fc.RightSideDescription,
                    Image = fc.RightSideImg,
                    Side = "Right",
                    FlashCardId = newFlashCardId,
                });

                sideFlashCardList.Add(new SideFlashCard
                {
                    Description = fc.LeftSideDescription,
                    Image = fc.LeftSideImg,
                    Side = "Left",
                    FlashCardId = newFlashCardId,
                });

                newFlashCard.SideFlashCards = sideFlashCardList;

                flashCardList.Add(newFlashCard);
            }

            return flashCardList;
        }

        private Question GenerateQuestion(Guid newQuestionPackageId, QuestionRequest quest, Guid newQuestionId)
        {
            return new Question
            {
                Id = newQuestionId,
                Description = quest.Description,
                Img = quest.Img,
                QuestionPacketId = newQuestionPackageId,
            };
        }

        private List<MultipleChoice> GenerateMutipleChoice(List<MutipleChoiceAnswerRequest> mcr, Guid newQuestionId)
        {
            var mutipleChoices = new List<MultipleChoice>();

            foreach (var answer in mcr)
            {
                mutipleChoices.Add(new MultipleChoice
                {
                    Description = answer.Description,
                    Img = answer.Img,
                    QuestionId = newQuestionId,
                    Score = answer.Score,
                });
            }

            return mutipleChoices;
        }

        public async Task<List<SyllabusResponse>> FilterSyllabusAsync(List<string>? keyWords, DateTime? date, double? score)
        {
            var numberOfSyll = (await _unitOfWork.GetRepository<Syllabus>().GetListAsync()).Count;
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

            var result = syllabuses.Select(syll => _mapper.Map<SyllabusResponse>(syll)).OrderByDescending(x => x.UpdateDate).ToList();
            foreach (var syll in result)
            {
                syll.NumberOfSyllabuses = numberOfSyll;
            }
            return result;
        }


        public async Task<SyllabusResponse> LoadSyllabusDynamicIdAsync(Guid courseId, Guid classId)
        {
            var syllabus = await ValidateSyllabus(courseId, true);
            var tempQuizTimes = new List<TempQuizTime>();

            if (classId != default)
            {
                var cls = await ValidateClassForSchedule(courseId, classId);
                var response = SyllabusCustomMapper.fromSyllabusAndClassToSyllabusResponseWithSheduleResponse(syllabus, cls);

                var questionPackages = syllabus.Topics!
                   .SelectMany(tp => tp.Sessions!.Where(ses => ses.QuestionPackage != null)
                   .Select(ses => ses!.QuestionPackage))
                   .ToList();

                //foreach (var session in syllabus.Topics!.SelectMany(tp => tp.Sessions!).ToList())
                //{
                //    var packages = await _unitOfWork.GetRepository<QuestionPackage>().GetListAsync(predicate: x => x.SessionId == session.Id);
                //    if (packages != null && packages.Count > 0)
                //    {
                //        questionPackages.AddRange(packages);
                //    }
                //}

                foreach (var session in response.SyllabusInformations!.Topics!.SelectMany(tp => tp.Sessions!).ToList())
                {
                    var quiz = questionPackages.SingleOrDefault(qp => session.OrderSession == qp!.NoSession);

                    if (quiz != null)
                    {
                        await SettingSessionExamTime(classId, tempQuizTimes, session, quiz, session);
                    }
                }
                try
                {
                    if (tempQuizTimes.Count > 0)
                    {
                        await _unitOfWork.GetRepository<TempQuizTime>().InsertRangeAsync(tempQuizTimes);
                        _unitOfWork.Commit();
                    }

                }
                catch (Exception ex)
                {
                    throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status500InternalServerError);
                }

                return response;
            }

            return _mapper.Map<SyllabusResponse>(syllabus);
        }

        private async Task SettingSessionExamTime(Guid classId, List<TempQuizTime> tempQuizTimes, SessionSyllabusResponse session, QuestionPackage quiz, SessionSyllabusResponse ssr)
        {
            var quizTime = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(predicate: x => x.ExamId == quiz.Id && x.ClassId == classId);

            var startTime = DateTime.Parse(session.Date!).Date.Add(session.StartTime!.Value.ToTimeSpan());
            var endTime = DateTime.Parse(session.Date!).Date.Add(session.EndTime!.Value.ToTimeSpan());
            int attempt = 1, duration = 600;
            bool isNonRequireTime = quiz.PackageType != PackageTypeEnum.Review.ToString() && quiz.PackageType != PackageTypeEnum.ProgressTest.ToString() ? false : true;

            if (quizTime != null)
            {
                startTime = quizTime.ExamStartTime != default ? startTime.Date.Add(quizTime.ExamStartTime) : startTime;
                endTime = quizTime.ExamEndTime != default ? endTime.Date.Add(quizTime.ExamEndTime) : endTime;
                attempt = quizTime.AttemptAllowed != 0 ? quizTime.AttemptAllowed : attempt;
                duration = quizTime.Duration != 0 ? quizTime.Duration : duration;
            }
            else if (!isNonRequireTime)
            {
                tempQuizTimes.Add(new TempQuizTime
                {
                    Id = Guid.NewGuid(),
                    ExamStartTime = session.StartTime!.Value.ToTimeSpan(),
                    ExamEndTime = session.EndTime!.Value.ToTimeSpan(),
                    AttemptAllowed = attempt,
                    Duration = duration,
                    ClassId = classId,
                    ExamId = quiz.Id,
                });
            }

            int part = quiz.QuizType.ToLower() == QuizTypeEnum.flashcard.ToString() ? 2 : 1;
            var extenstionName = quiz.PackageType == PackageTypeEnum.FinalExam.ToString() ? "" : " " + quiz.OrderPackage;

            ssr.Quiz = new QuizInforResponse
            {
                ExamId = quiz.Id,
                ExamName = "Bài " + quiz.ContentName.ToLower() + extenstionName,
                ExamPart = part,
                QuizName = quiz.Score == 0 ? "Làm Tại Lớp" : quiz.Title!,
                QuizDuration = isNonRequireTime ? null : duration,
                Attempts = isNonRequireTime ? null : attempt,
                QuizStartTime = isNonRequireTime ? null : startTime,
                QuizEndTime = isNonRequireTime ? null : endTime,
            };
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
                var pakage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id);
                if (pakage != null)
                {
                    session.QuestionPackage = pakage;
                }

            }

            return syllabus;
        }

        private async Task<Syllabus> CheckingIdRequest(Guid id, bool isCourseId)
        {
            var syllabus = new Syllabus();

            if (isCourseId)
            {
                syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(predicate: x => x.Course!.Id == id,
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
            if (role == null)
            {
                return syllabuses.ToList();
            }
            if (role.ToLower() == RoleEnum.LECTURER.ToString().ToLower())
            {
                var coursesOfLecturer = await _unitOfWork.GetRepository<Course>().GetListAsync(
                    selector: x => x.SyllabusId,
                    predicate: x => x.Classes.Any(cls => cls.LecturerId == GetUserIdFromJwt()));

                syllabuses = syllabuses.Where(syll => coursesOfLecturer.Any(id => id == syll.Id)).ToList();
            }

            if (role.ToLower() == RoleEnum.STUDENT.ToString().ToLower())
            {
                var coursesOfStudent = await _unitOfWork.GetRepository<Class>().GetListAsync(
                    selector: x => x.Course.SyllabusId,
                    predicate: x => x.StudentClasses.Any(sc => sc.StudentId == GetUserIdFromJwt()));

                syllabuses = syllabuses.Where(syll => coursesOfStudent.Any(id => id == syll.Id)).ToList();
            }

            return syllabuses.ToList();
        }

        #endregion
        #region thuong code
        public async Task<List<SyllabusResponseV2>> GetAllSyllabus(string? keyword = null)
        {
            var syllabuses = await _unitOfWork.GetRepository<Syllabus>().GetListAsync();
            List<SyllabusResponseV2> responses = new List<SyllabusResponseV2>();
            foreach (var syl in syllabuses)
            {
                var name = "undefined";
                var subjectCode = "undefined";
                var syllabusName = "undefined";

                var sylCourse = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.SyllabusId == syl.Id);
                if (sylCourse != null)
                {
                    name = sylCourse.Name;
                }
                if (syl.SubjectCode != null)
                {
                    subjectCode = syl.SubjectCode;
                }
                if (syl.Name != null)
                {
                    syllabusName = syl.Name;
                }

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
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                include: x => x.Include(x => x.Course!),
                predicate: x => x.Id.ToString().Equals(id));

            if (syllabus.Course == null)
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
                    syllabus.MinAvgMarkToPass = request.MinAvgMarkToPass.Value;
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
                    syllabus.ScoringScale = request.ScoringScale.Value;
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

            var exams = GenerateExam(request.ExamSyllabusRequests, syllabusId);
            if (exams.Count > 0)
            {
                await _unitOfWork.GetRepository<ExamSyllabus>().InsertRangeAsync(exams);
            }

            await _unitOfWork.CommitAsync();
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

            var sessionInsert = GenerateLearningItems(request.SyllabusRequests, syllabusId);
            var items = GenerateExerciseItemStaff(request.QuestionPackageRequests!, sessionInsert.SelectMany(tp => tp.Sessions).ToList());
            await _unitOfWork.GetRepository<QuestionPackage>().InsertRangeAsync(items);
            await _unitOfWork.CommitAsync();

        }

        private List<QuestionPackage> GenerateExerciseItemStaff(List<QuestionPackageRequest> questionPackageRequest, List<Session> sessions)
        {
            int orderPackage = 0, noLastSession = 0;

            questionPackageRequest = questionPackageRequest.OrderBy(qp => qp.NoOfSession).ToList();
            var questionPackages = new List<QuestionPackage>();

            noLastSession = questionPackageRequest.Max(qp => qp.NoOfSession);
            foreach (var qpr in questionPackageRequest)
            {
                orderPackage++;
                Guid newQuestionPackageId = Guid.NewGuid();

                var questionPackage = GenerateQuestionPackageStaff(sessions, qpr, newQuestionPackageId, orderPackage, noLastSession);

                if (qpr.QuestionRequests.Count > 0)
                {
                    var questions = GenerateQuestionPackgeItems(newQuestionPackageId, qpr.QuestionRequests);
                    questionPackage.Questions = questions;
                }

                questionPackages.Add(questionPackage);
            }

            return questionPackages;

        }
        public async Task<StaffSyllabusResponse> GetStaffSyllabusResponse(string id)
        {
            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Id.ToString().Equals(id),
                include: x => x.Include(x => x.Course)!);

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
            var courseId = syllabus.Course != null ? syllabus.Course.Id : default;
            if (courseId == default)
            {
                syllRes.LinkedCourse = null;
            }
            else
            {
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseId.ToString()));
                var courseName = course.Name;
                syllRes.LinkedCourse = new PayLoad.Response.Courses.LinkedCourse
                {
                    CourseId = courseId,
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


        ///-----///
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
                Type = questionpackage.QuizType,
                //Deadline = questionpackage.DeadlineTime,
                //Duration = questionpackage.Duration,
                Score = questionpackage.Score,
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
                if (syl.CourseName.Trim().ToLower().Equals("undefined") && (syl.EffectiveDate.Value.AddDays(-2) < DateTime.Now))
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
                questionPackage.QuizType = request.Type;
            }
            if (request.Score != null)
            {
                questionPackage.Score = request.Score.Value;
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

            var items = GenerateQuestionPackgeItems(questionPackage.Id, request.QuestionRequests);
            await _unitOfWork.GetRepository<Question>().InsertRangeAsync(items);
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
            var courseId = syllabus.Course != null ? syllabus.Course.Id : default;
            if (courseId == default)
            {
                syllRes.LinkedCourse = null;
            }
            else
            {
                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseId.ToString()));
                var courseName = course.Name;
                syllRes.LinkedCourse = new PayLoad.Response.Courses.LinkedCourse
                {
                    CourseId = courseId,
                    CourseName = courseName
                };
            }
            return syllRes;
        }

        public async Task<SyllabusResultResponse> FilterStaffSyllabusAsync(List<string>? keyWords, DateTime? date, double? score)
        {
            var numberOfSyll = (await _unitOfWork.GetRepository<Syllabus>().GetListAsync()).Count;
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

            var result = syllabuses.Select(syll => _mapper.Map<SyllabusResponse>(syll)).OrderByDescending(x => x.UpdateDate).ToList();
            foreach (var syll in result)
            {
                syll.NumberOfSyllabuses = numberOfSyll;
            }
            return new SyllabusResultResponse
            {
                NumberOfSyllabus = numberOfSyll,
                Syllabuses = result
            };
        }


        #endregion


    }

}
