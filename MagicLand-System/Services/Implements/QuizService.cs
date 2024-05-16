using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain.Models.TempEntity.Class;
using MagicLand_System.Domain.Models.TempEntity.Quiz;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request.Quizzes;
using MagicLand_System.PayLoad.Response.Custom;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
using MagicLand_System.PayLoad.Response.Quizzes.Result;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Final;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Student;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class QuizService : BaseService<QuizService>, IQuizService
    {
        public QuizService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<QuizService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
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

        public async Task<List<ExamResForStudent>> LoadExamOfClassByClassIdAsync(Guid classId, Guid? studentId)
        {
            var examsResponse = new List<ExamResponse>();

            var cls = await ValidateClass(classId, studentId);

            var sessions = cls.Course!.Syllabus!.Topics!.SelectMany(tp => tp.Sessions!).ToList();
            foreach (var session in sessions)
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

                    res.Score = isQuizDone != null ? isQuizDone.ScoreEarned : null;
                    res.ExamStatus = isQuizDone != null ? isQuizDone.ExamStatus : null;

                    if (attemptSetting != 0)
                    {
                        res.AttemptLeft = isQuizDone != null ? attemptSetting - isQuizDone.NoAttempt < 0 ? 0 : attemptSetting - isQuizDone.NoAttempt : attemptSetting;
                    }
                    else
                    {
                        var packageType = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(
                        selector: x => x.PackageType,
                        predicate: x => x.Id == res.ExamId);

                        if (packageType != PackageTypeEnum.Review.ToString())
                        {
                            res.AttemptLeft = isQuizDone != null ? 0 : 1;
                        }
                        else
                        {
                            res.AttemptLeft = null;
                        }
                    }
                }
            }

            return responses;
        }

        private async Task GenerateExamWithDate(List<ExamResponse> examsResponse, Class cls, Session session)
        {
            var quizzes = await _unitOfWork.GetRepository<QuestionPackage>().GetListAsync(predicate: x => x.SessionId == session.Id);

            if (quizzes == null || quizzes.Count == 0)
            {
                return;
            }
            var tempQuizTimes = new List<TempQuizTime>();

            foreach (var quiz in quizzes)
            {
                quiz.Questions = (await _unitOfWork.GetRepository<Question>().GetListAsync(
                predicate: x => x.QuestionPacketId == quiz.Id,
                include: x => x.Include(x => x.MutipleChoices).Include(x => x.FlashCards!).ThenInclude(fc => fc.SideFlashCards!))).ToList();

                var exam = cls.Course!.Syllabus!.ExamSyllabuses!.SingleOrDefault(exam => StringHelper.TrimStringAndNoSpace(exam.ContentName!) == StringHelper.TrimStringAndNoSpace(quiz.ContentName!));

                var examResponse = QuizCustomMapper.fromSyllabusItemsToExamResponse(quiz, exam);
                var quizTime = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(predicate: x => x.ExamId == quiz.Id && x.ClassId == cls.Id);

                SettingExamTimeInDate(examsResponse, cls, session, quiz, tempQuizTimes, examResponse, quizTime);
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
                throw new BadHttpRequestException($"Lỗi Hệ Thống Không Phát Sinh [{ex.Message}]",
                   StatusCodes.Status500InternalServerError);
            }
        }

        private void SettingExamTimeInDate(List<ExamResponse> examsResponse, Class cls, Session session, QuestionPackage quiz, List<TempQuizTime> tempQuizTimes, ExamResponse examResponse, TempQuizTime quizTime)
        {
            var schedule = cls.Schedules.ToList()[session.NoSession - 1];
            var date = schedule.Date.ToString("yyyy-MM-ddTHH:mm:ss");
            var startTime = DateTime.Parse(date).Date.Add(TimeSpan.Parse(schedule.Slot!.StartTime));
            var endTime = DateTime.Parse(date).Date.Add(TimeSpan.Parse(schedule.Slot!.EndTime));
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
                    ExamStartTime = TimeSpan.Parse(schedule.Slot!.StartTime),
                    ExamEndTime = TimeSpan.Parse(schedule.Slot!.EndTime),
                    AttemptAllowed = attempt,
                    Duration = duration,
                    ClassId = cls.Id,
                    ExamId = quiz.Id,
                });
            }


            examResponse.SessionId = session.Id;
            examResponse.CourseId = cls.Course!.Id;
            examResponse.Date = date;

            examResponse.AttemptAlloweds = isNonRequireTime ? null : attempt;
            //examResponse.ExamStartTime = isNonRequireTime ? null : startTime;
            //examResponse.ExamEndTime = isNonRequireTime ? null : endTime;
            examResponse.ExamStartTime = startTime;
            examResponse.ExamEndTime = endTime;



            //examResponse.ExamStartTime = isNonRequireTime ? startTime.Date.Add(new TimeSpan(6, 0, 0)) : startTime;
            //examResponse.ExamEndTime = isNonRequireTime ? endTime.Date.Add(new TimeSpan(23, 59, 0)) : endTime;

            //examResponse.Duration = isNonRequireTime ? null : duration;
            examResponse.Duration = duration;

            examsResponse.Add(examResponse);
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
                if (studentClass.SavedTime != null)
                {
                    throw new BadHttpRequestException($"Id [{studentId}] Của Học Sinh Thuộc Lớp Học Này, Đã Bảo Lưu Không Thể Truy Suất", StatusCodes.Status400BadRequest);
                }
            }

            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
             predicate: x => x.Course!.Id == cls.CourseId,
             include: x => x.Include(syll => syll!.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))!.Include(syll => syll!.ExamSyllabuses!));

            cls.Course!.Syllabus = syllabus;

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

        public async Task<List<ExamExtraClassInfor>> LoadExamOfCurrentStudentAsync(int numberOfDate)
        {
            var studentId = (await GetUserFromJwt()).StudentIdAccount;
            if (studentId == null)
            {
                throw new BadHttpRequestException("Lỗi Hệ Thống Không Thể Xác Thực Người Dùng, Vui Lòng Đăng Nhập Lại Và Thực Hiện Lại Thao Tác",
                    StatusCodes.Status500InternalServerError);
            }

            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.StudentClasses.Any(sc => sc.StudentId == studentId && sc.SavedTime == null) && x.Status == ClassStatusEnum.PROGRESSING.ToString());

            if (!classes.Any())
            {
                throw new BadHttpRequestException("Bé Chưa Tham Gia Lớp Học Nào Hoặc Lơp Học Tham Gia Đã Bảo Lưu Không Thể Truy Suất", StatusCodes.Status400BadRequest);
            }
            var responses = new List<ExamExtraClassInfor>();
            foreach (var cls in classes)
            {
                var examsResponse = new List<ExamResponse>();

                cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                    orderBy: x => x.OrderBy(x => x.Date),
                    predicate: x => x.ClassId == cls.Id,
                    include: x => x.Include(x => x.Slot).Include(x => x.Room)!);

                cls.Course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == cls.CourseId);

                cls.Course!.Syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == cls.CourseId,
                include: x => x.Include(syll => syll!.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession))!.Include(syll => syll!.ExamSyllabuses!));

                foreach (var session in cls.Course!.Syllabus!.Topics!.SelectMany(tp => tp.Sessions!).ToList())
                {
                    await GenerateExamWithDate(examsResponse, cls, session);
                }

                await SettingExamInfor(numberOfDate, studentId.Value, responses, cls, examsResponse);
            }

            return responses;
        }

        private async Task SettingExamInfor(int numberOfDate, Guid studentId, List<ExamExtraClassInfor> responses, Class cls, List<ExamResponse> examsResponse)
        {
            foreach (var exam in examsResponse)
            {
                string status = string.Empty;
                var examDate = DateTime.Parse(exam.Date!).Date;
                var currentDate = GetCurrentTime().Date;

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
                    responses.Add(new ExamExtraClassInfor
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
                        Duration = exam.Duration,
                        AttemptAlloweds = exam.AttemptAlloweds,
                        ExamStartTime = exam.ExamStartTime,
                        ExamEndTime = exam.ExamEndTime,
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

        public async Task<List<QuizResponse>> LoadQuizOfExamByExamIdAsync(Guid examId, Guid classId, int? examPart)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == classId,
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Không Tồn Tại Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

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

            await ValidateDayDoingExam(examId, cls, quiz);

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

        private async Task ValidateDayDoingExam(Guid examId, Class cls, QuestionPackage quiz)
        {
            var sessions = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == cls.CourseId,
                selector: x => x.Topics!.SelectMany(tp => tp.Sessions!));

            if (!sessions.Any(ses => ses.Id == quiz.SessionId))
            {
                throw new BadHttpRequestException($"Id [{examId}] Của Bài Kiểm Tra Không Thuộc Lớp Đang Truy Suất", StatusCodes.Status400BadRequest);
            }

            //var dayDoingExam = cls.Schedules.ToList()[quiz.NoSession - 1].Date;

            //if (dayDoingExam.Date > GetCurrentTime().Date)
            //{
            //    throw new BadHttpRequestException($"Id [{examId}] Của Bài Kiểm Tra Vẫn Chưa Tới Ngày Làm Bài Không Thể Truy Suất Câu Hỏi", StatusCodes.Status400BadRequest);
            //}
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
                var tempFCAnswers = new List<TempFCAnswer>();

                Guid tempQuizId = Guid.NewGuid();
                var tempQuiz = new TempQuiz
                {
                    Id = tempQuizId,
                    ExamId = examId,
                    StudentId = (await GetUserFromJwt()).StudentIdAccount!.Value,
                    TotalMark = totalMark,
                    ExamType = quiz.QuizType,
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
                    var flashCardAnswers = res.AnwserFlashCarsInfor;
                    if (flashCardAnswers != null && flashCardAnswers.Count > 0)
                    {
                        foreach (var answer in flashCardAnswers)
                        {
                            tempFCAnswers.Add(new TempFCAnswer
                            {
                                Id = Guid.NewGuid(),
                                CardId = answer.CardId,
                                Score = answer.Score,
                                NumberCoupleIdentify = answer.NumberCoupleIdentify,
                                TempQuestionId = tempQuestionId,
                            });
                        }
                    }
                }

                await _unitOfWork.GetRepository<TempQuiz>().InsertAsync(tempQuiz);
                await _unitOfWork.GetRepository<TempQuestion>().InsertRangeAsync(tempQuestions);
                if (tempMCAnswers.Any())
                {
                    await _unitOfWork.GetRepository<TempMCAnswer>().InsertRangeAsync(tempMCAnswers);
                }
                if (tempFCAnswers.Any())
                {
                    await _unitOfWork.GetRepository<TempFCAnswer>().InsertRangeAsync(tempFCAnswers);
                }
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status400BadRequest);
            }
        }

        public async Task<QuizResultResponse> GradeQuizMCAsync(QuizMCRequest quizStudentWork, TimeOnly doingTime, bool? isCheckingTime)
        {
            var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

            var cls = await ValidateGradeQuizClass(quizStudentWork.ClassId, currentStudentId);

            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == cls.CourseId,
                include: x => x.Include(x => x.ExamSyllabuses)!);

            var quizzes = new List<QuestionPackage>();

            var sessions = (await _unitOfWork.GetRepository<Topic>().GetListAsync(
                predicate: x => x.SyllabusId == syllabus.Id,
                include: x => x.Include(x => x.Sessions!))).SelectMany(x => x.Sessions!).ToList();

            foreach (var ses in sessions)
            {
                var package = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == ses.Id);
                if (package != null)
                {
                    quizzes.Add(package);
                }
            }

            var currentQuiz = quizzes.Find(q => q!.Id == quizStudentWork.ExamId);

            ValidateGradeCurrentQuiz(quizStudentWork.ExamId, currentQuiz, cls.Schedules.ToList(), doingTime, false, isCheckingTime);

            var currentTempQuiz = await _unitOfWork.GetRepository<TempQuiz>().SingleOrDefaultAsync(
                orderBy: x => x.OrderByDescending(x => x.CreatedTime),
                predicate: x => x.StudentId == currentStudentId && x.ExamId == currentQuiz!.Id,
                include: x => x.Include(x => x.Questions).ThenInclude(qt => qt.MCAnswers));

            if (currentTempQuiz == null)
            {
                throw new BadHttpRequestException($"Bài Làm Không Hợp Lệ Khi Không Truy Suất Được Gói Câu Hỏi Trong Hệ Thống, Vui Lòng Truy Suất Lại Gói Câu Hỏi Và Làm Lại",
                    StatusCodes.Status400BadRequest);
            }

            if (currentTempQuiz.IsGraded == true)
            {
                throw new BadHttpRequestException($"Gói Câu Hỏi Của Bài Kiểm Tra Đã Được Chấm Điểm Làm Vui Lòng Tuy Suất Gói Câu Hỏi Khác", StatusCodes.Status400BadRequest);
            }

            Guid testResultId;
            TestResult testResult;

            if (quizStudentWork.StudentQuestionResults.Count > 0)
            {
                ValidateTempQuizDB(quizStudentWork.StudentQuestionResults.Count(), currentQuiz, currentTempQuiz, false);
            }

            var questionItems = ValidateStudentMCWorkRequest(quizStudentWork, currentTempQuiz);

            int noAttempt = await GetAttempt(quizStudentWork.ExamId, currentStudentId, cls.Id, currentQuiz!.PackageType);

            GenrateTestResult(syllabus, currentQuiz, currentTempQuiz.TotalMark, cls.StudentClasses.ToList().Find(sc => sc.StudentId == currentStudentId)!, noAttempt, out testResultId, out testResult);

            var examQuestions = new List<ExamQuestion>();
            var multipleChoiceAnswers = new List<MultipleChoiceAnswer>();
            int correctMark = 0;
            double scoreEarned = 0;
            string status = "Chưa Có Đánh Giá";//GenerateExamStatus(testResult.TotalMark, correctMark);

            if (quizStudentWork.StudentQuestionResults.Count > 0)
            {
                foreach (var sqr in quizStudentWork.StudentQuestionResults)
                {
                    var currentTempQuestion = questionItems.Item1.Find(q => q.QuestionId == sqr.QuestionId);

                    var currentQuestion = await _unitOfWork.GetRepository<Question>().SingleOrDefaultAsync(
                    predicate: x => x.Id == currentTempQuestion!.QuestionId,
                    include: x => x.Include(x => x.MutipleChoices!));

                    var currentAnswer = currentTempQuestion!.MCAnswers.ToList().Find(a => a.AnswerId == sqr.AnswerId);
                    var currentCorrectAnswer = currentTempQuestion!.MCAnswers.ToList().Find(a => a.Score != 0);

                    correctMark += currentAnswer!.Score != 0 ? +1 : 0;
                    scoreEarned += currentAnswer!.Score != 0 ? currentAnswer.Score : 0;

                    var answer = currentQuestion.MutipleChoices!.Find(mc => mc.Id == currentAnswer!.AnswerId);
                    var correctAnswer = currentQuestion.MutipleChoices!.Find(mc => mc.Id == currentCorrectAnswer!.AnswerId);

                    GenerateMCResultItems(testResultId, examQuestions, multipleChoiceAnswers, sqr, currentQuestion, currentAnswer, answer, correctAnswer);
                }
            }


            await GenerateMCResultNonAnswerItems(questionItems.Item2, testResultId, examQuestions, multipleChoiceAnswers);

            var response = SettingLastResultInfor(doingTime, testResult, correctMark, scoreEarned, status, false);

            await SaveGrading(currentTempQuiz, testResult, examQuestions, multipleChoiceAnswers, null);

            return response;
        }

        private async Task GenerateMCResultNonAnswerItems(List<TempQuestion> nonAnswerTempQuestion, Guid testResultId, List<ExamQuestion> examQuestions, List<MultipleChoiceAnswer> multipleChoiceAnswers)
        {
            if (nonAnswerTempQuestion.Any() && nonAnswerTempQuestion != null)
            {
                foreach (var tempQuestion in nonAnswerTempQuestion)
                {
                    var currentNonAnswerQuestion = await _unitOfWork.GetRepository<Question>().SingleOrDefaultAsync(
                        predicate: x => x.Id == tempQuestion.QuestionId,
                        include: x => x.Include(x => x.MutipleChoices!));

                    var currentCorrectAnswer = currentNonAnswerQuestion!.MutipleChoices!.ToList().Find(a => a.Score != 0);

                    Guid examQuestionId = Guid.NewGuid();
                    examQuestions.Add(new ExamQuestion
                    {
                        Id = examQuestionId,
                        QuestionId = currentNonAnswerQuestion.Id,
                        Question = currentNonAnswerQuestion.Description,
                        QuestionImage = currentNonAnswerQuestion.Img,
                        TestResultId = testResultId,
                    });

                    multipleChoiceAnswers.Add(new MultipleChoiceAnswer
                    {
                        Id = Guid.NewGuid(),
                        AnswerId = default,
                        Answer = null,
                        AnswerImage = null,
                        CorrectAnswerId = currentCorrectAnswer!.Id,
                        CorrectAnswer = currentCorrectAnswer.Description,
                        CorrectAnswerImage = currentCorrectAnswer.Img,
                        Status = "NotAnswer",
                        Score = 0,
                        ExamQuestionId = examQuestionId,
                    });
                }
            }
        }

        private void GenerateMCResultItems(Guid testResultId, List<ExamQuestion> examQuestions, List<MultipleChoiceAnswer> multipleChoiceAnswers,
            MCStudentAnswer sqr, Question question, TempMCAnswer? currentAnswer, MultipleChoice? answer, MultipleChoice? correctAnswer)
        {
            Guid examQuestionId = Guid.NewGuid();
            examQuestions.Add(new ExamQuestion
            {
                Id = examQuestionId,
                QuestionId = question.Id,
                Question = question.Description,
                QuestionImage = question.Img,
                TestResultId = testResultId,
            });

            multipleChoiceAnswers.Add(new MultipleChoiceAnswer
            {
                Id = Guid.NewGuid(),
                AnswerId = sqr.AnswerId,
                Answer = answer!.Description,
                AnswerImage = answer.Img != null ? answer.Img : string.Empty,
                CorrectAnswerId = correctAnswer!.Id,
                CorrectAnswer = correctAnswer.Description,
                CorrectAnswerImage = correctAnswer.Img,
                Status = currentAnswer!.Score != 0 ? "Correct" : "Wrong",
                Score = currentAnswer.Score,
                ExamQuestionId = examQuestionId,
            });
        }

        private void GenerateFCResultItems(Guid examQuestionId, List<FlashCardAnswer> flashCardAnswers,
            SideFlashCard firstCardAnswer, SideFlashCard secondCardAnswer, SideFlashCard correctSecondCardAnswer, string status, double score)
        {
            flashCardAnswers.Add(new FlashCardAnswer
            {
                Id = Guid.NewGuid(),
                LeftCardAnswerId = firstCardAnswer.Id,
                LeftCardAnswer = firstCardAnswer.Description,
                LeftCardAnswerImage = firstCardAnswer.Image,
                RightCardAnswerId = secondCardAnswer.Id,
                RightCardAnswer = secondCardAnswer.Description,
                RightCardAnswerImage = secondCardAnswer.Image,
                CorrectRightCardAnswerId = correctSecondCardAnswer.Id,
                CorrectRightCardAnswer = correctSecondCardAnswer.Description,
                CorrectRightCardAnswerImage = correctSecondCardAnswer.Image,
                ExamQuestionId = examQuestionId,
                Status = status,
                Score = score,
            });
        }


        private async Task SaveGrading(TempQuiz currentTempQuiz, TestResult testResult, List<ExamQuestion>? examQuestions, List<MultipleChoiceAnswer>? multipleChoiceAnswers, List<FlashCardAnswer>? flashCardAnswers)
        {
            try
            {
                if (multipleChoiceAnswers != null)
                {
                    foreach (var mc in multipleChoiceAnswers)
                    {
                        var matchingQuestions = from eq in examQuestions
                                                where eq.Id == mc.ExamQuestionId
                                                select eq;

                        foreach (var eq in matchingQuestions)
                        {
                            eq.MultipleChoiceAnswerId = mc.Id;
                        }
                    }
                    await _unitOfWork.GetRepository<MultipleChoiceAnswer>().InsertRangeAsync(multipleChoiceAnswers);
                }

                if (flashCardAnswers != null)
                {
                    await _unitOfWork.GetRepository<FlashCardAnswer>().InsertRangeAsync(flashCardAnswers);
                }

                currentTempQuiz.IsGraded = true;
                _unitOfWork.GetRepository<TempQuiz>().UpdateAsync(currentTempQuiz);
                await _unitOfWork.GetRepository<TestResult>().InsertAsync(testResult);
                if (examQuestions != null)
                {
                    await _unitOfWork.GetRepository<ExamQuestion>().InsertRangeAsync(examQuestions);
                }
                _unitOfWork.Commit();

            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]" + ex.InnerException != null ? $"Inner: [{ex.InnerException}]" : "", StatusCodes.Status500InternalServerError);
            }
        }

        //private string GenerateExamStatus(int totalMark, int correctMark)
        //{
        //    string status = string.Empty;
        //    double percentage = (double)correctMark / (double)totalMark;
        //    percentage = percentage * 100;

        //    if (percentage < 50)
        //    {
        //        status = "Not Good";
        //    }
        //    if (percentage >= 50 && percentage <= 70)
        //    {
        //        status = "Good";
        //    }
        //    if (percentage > 70)
        //    {
        //        status = "Excellent";
        //    }

        //    return status;
        //}

        private void GenrateTestResult(Syllabus syllabus, QuestionPackage? currentQuiz, int totalMark, StudentClass studentClass, int noAttempt, out Guid testResultId, out TestResult testResult)
        {
            var currentExam = syllabus.ExamSyllabuses!.SingleOrDefault(es => es.ContentName!.Trim().ToLower() == currentQuiz!.ContentName!.Trim().ToLower());

            testResultId = Guid.NewGuid();
            testResult = new TestResult
            {
                Id = testResultId,
                ExamId = currentQuiz!.Id,
                ExamName = "Bài Kiểm Tra Số " + currentQuiz.OrderPackage,
                QuizCategory = currentExam != null ? currentExam.Category : PackageTypeEnum.Review.ToString(),
                QuizType = currentQuiz.QuizType,
                QuizName = currentQuiz.Title,
                TotalScore = currentQuiz.Score,
                TotalMark = totalMark,
                StudentClassId = studentClass.Id,
                NoAttempt = noAttempt,
            };
        }

        private async Task<int> GetAttempt(Guid examId, Guid? currentStudentId, Guid classId, string packageType)
        {
            int noAttempt = 1;
            var isExamHasDone = await _unitOfWork.GetRepository<TestResult>().GetListAsync(
                orderBy: x => x.OrderByDescending(x => x.NoAttempt),
                predicate: x => x.StudentClass!.StudentId == currentStudentId && x.StudentClass.ClassId == classId && x.ExamId == examId);

            var quizTime = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(
                predicate: x => x.ExamId == examId && x.ClassId == classId);

            if (isExamHasDone != null && isExamHasDone.Any())
            {
                if (quizTime is null && packageType != PackageTypeEnum.Review.ToString())
                {
                    throw new BadHttpRequestException($"Bạn Đã Làm Vượt Quá Số Lần Cho Phép Của Bài Kiểm Tra", StatusCodes.Status400BadRequest);
                }

                if (quizTime is null && packageType == PackageTypeEnum.Review.ToString())
                {
                    noAttempt = isExamHasDone.First().NoAttempt + 1;
                }

                if (quizTime is not null && (isExamHasDone.Count() < quizTime.AttemptAllowed))
                {
                    noAttempt = isExamHasDone.First().NoAttempt + 1;
                }
                if (quizTime is not null && (isExamHasDone.Count() >= quizTime.AttemptAllowed))
                {
                    throw new BadHttpRequestException($"Bạn Đã Làm Đủ Số Lần Cho Phép Của Bài Kiểm Tra", StatusCodes.Status400BadRequest);
                }
            }

            return noAttempt;
        }

        private (List<TempQuestion>, List<TempQuestion>) ValidateStudentMCWorkRequest(QuizMCRequest quizStudentWork, TempQuiz currentTempQuiz)
        {
            var questionRequest = quizStudentWork.StudentQuestionResults.Select(sq => sq.QuestionId).ToList();
            var questions = currentTempQuiz.Questions.ToList();


            var invalidQuestion = questionRequest.Where(qr => !questions.Any(q => q.QuestionId == qr)).ToList();
            if (invalidQuestion != null && invalidQuestion.Any())
            {
                throw new BadHttpRequestException($"Một Số Id Câu Hỏi Không Hợp Lệ Khi Không Thuộc Gói Câu Hỏi Của Bài Kiểm Tra, [{string.Join(", ", invalidQuestion)}]",
                          StatusCodes.Status400BadRequest);
            }

            var answerRequest = quizStudentWork.StudentQuestionResults.Select(sq => sq.AnswerId).ToList();
            var answers = questions.SelectMany(qt => qt.MCAnswers!).ToList();

            var invalidAnswer = answerRequest.Where(ar => !answers.Any(a => a.AnswerId == ar)).ToList();

            if (invalidAnswer != null && invalidAnswer.Any())
            {
                throw new BadHttpRequestException($"Một Số Id Câu Trả Lời Không Hợp Lệ Khi Không Thuộc Gói Câu Trả Lời Của Bài Kiểm Tra, [{string.Join(", ", invalidAnswer)}]",
                          StatusCodes.Status400BadRequest);
            }

            var questionNotAnswer = questions.Where(q => !questionRequest.Any(qr => qr == q.QuestionId)).ToList();

            return (questions, questionNotAnswer);
        }

        private (List<TempQuestion>, List<TempQuestion>, List<(List<TempFCAnswer>, Guid)>) ValidateStudentFCWorkRequest(QuizFCRequest quizStudentWork, TempQuiz currentTempQuiz)
        {
            var questionRequest = quizStudentWork.StudentQuestionResults.Select(sq => sq.QuestionId).ToList();
            var questions = currentTempQuiz.Questions.ToList();


            var invalidQuestion = questionRequest.Where(qr => !questions.Any(q => q.QuestionId == qr)).ToList();
            if (invalidQuestion != null && invalidQuestion.Any())
            {
                throw new BadHttpRequestException($"Một Số Id Câu Hỏi Không Hợp Lệ Khi Không Thuộc Gói Câu Hỏi Của Bài Kiểm Tra, [{string.Join(", ", invalidQuestion)}]",
                          StatusCodes.Status400BadRequest);
            }

            var answerRequest = quizStudentWork.StudentQuestionResults.SelectMany(sq => sq.Answers).ToList();
            var flashCards = new List<Guid>();
            foreach (var ar in answerRequest)
            {
                flashCards.Add(ar.FirstCardId);
                flashCards.Add(ar.SecondCardId);
            }

            var answers = questions.SelectMany(qt => qt.FCAnswers!).ToList();

            var invalidAnswer = flashCards.Where(fc => !answers.Any(a => a.CardId == fc)).ToList();

            if (invalidAnswer != null && invalidAnswer.Any())
            {
                throw new BadHttpRequestException($"Một Số Id Của Thẻ Trả Lời Không Hợp Lệ Khi Không Thuộc Gói Câu Hỏi Đã Truy Suất Gần Nhất Của Bài Kiểm Tra, [{string.Join(", ", invalidAnswer)}]",
                          StatusCodes.Status400BadRequest);
            }

            var questionNotAnswers = new List<TempQuestion>();
            var flashCardNotAnswers = new List<(List<TempFCAnswer>, Guid)>();

            foreach (var quest in questions)
            {
                //if (!questionRequest.Any(qr => qr == quest.QuestionId))
                //{

                //}
                var currentQuestRequest = quizStudentWork.StudentQuestionResults.Find(qr => qr.QuestionId == quest.QuestionId);
                if (currentQuestRequest == null)
                {
                    questionNotAnswers.Add(quest);
                    continue;
                }
                if (currentQuestRequest.Answers == null || !currentQuestRequest.Answers.Any())
                {
                    questionNotAnswers.Add(quest);
                    continue;
                }

                var allAnswerCurrentQuestion = currentTempQuiz.Questions.SelectMany(q => q.FCAnswers.Where(fc => fc.TempQuestionId == quest.Id)).ToList();
                var allCoupleCardAnswerRequest = quizStudentWork.StudentQuestionResults.Where(sqr => sqr.QuestionId == quest.QuestionId).SelectMany(sqr => sqr.Answers).ToList();
                var allCardAnswerRequest = new List<Guid>();

                foreach (var ar in allCoupleCardAnswerRequest)
                {
                    allCardAnswerRequest.Add(ar.FirstCardId);
                    allCardAnswerRequest.Add(ar.SecondCardId);
                }

                var cardNotAnswers = allAnswerCurrentQuestion.Where(aacq => !allCardAnswerRequest.Any(acar => acar == aacq.CardId)).ToList();

                if (cardNotAnswers.Any())
                {
                    flashCardNotAnswers.Add((cardNotAnswers, quest.QuestionId));
                }
            }

            return (questions, questionNotAnswers, flashCardNotAnswers);
        }

        private void ValidateTempQuizDB(int totalQuestion, QuestionPackage? currentQuiz, TempQuiz currentTempQuiz, bool isFlashCard)
        {

            if (currentTempQuiz.ExamType!.Trim().ToLower() != currentQuiz!.QuizType!.Trim().ToLower())
            {
                throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh Gói Câu Hỏi Không Thuộc Dạng Đề Của Bài Kiểm Tra, Vui Lòng Chờ Sử Lý",
                          StatusCodes.Status500InternalServerError);
            }

            if (!isFlashCard)
            {
                if (totalQuestion > currentTempQuiz.TotalMark)
                {
                    throw new BadHttpRequestException("Số Lượng Câu Hỏi Và Trả Lời Bài Làm Của Học Sinh Lớn Hơn Với Số Lượng Câu Hỏi Và Câu Trả Lời Bộ Đề Đã Truy Suất Gần Nhất Vui Lòng Xem Lại Bài Làm",
                             StatusCodes.Status400BadRequest);
                }
            }

        }

        private void ValidateGradeCurrentQuiz(Guid examId, QuestionPackage? currentQuiz, List<Schedule> schedules, TimeOnly doingTime, bool isFlashCard, bool? isCheckingTime)
        {
            if (currentQuiz == null)
            {
                throw new BadHttpRequestException($"Id [{examId}] Bài Kiểm Tra Không Tồn Tại Hoặc Không Thuộc Lớp Đang Yêu Cầu Truy Vấn",
                          StatusCodes.Status400BadRequest);
            }

            if (String.Compare(currentQuiz.PackageType, PackageTypeEnum.ProgressTest.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new BadHttpRequestException($"Id [{examId}] Bài Kiểm Tra Thuộc Dạng Tự Làm Tại Nhà Cần Nhập Điểm Trực Tiếp",
                         StatusCodes.Status400BadRequest);
            }

            if (isFlashCard)
            {
                if (currentQuiz.QuizType.ToLower() != QuizTypeEnum.flashcard.ToString())
                {
                    throw new BadHttpRequestException($"Id [{examId}] Bài Kiểm Tra Thuộc Dạng Trắc Nghiệm, Yêu Cầu Không Hợp Lệ",
                             StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                if (currentQuiz.QuizType.ToLower() == QuizTypeEnum.flashcard.ToString())
                {
                    throw new BadHttpRequestException($"Id [{examId}] Bài Kiểm Tra Thuộc Dạng Nối Thẻ, Yêu Cầu Không Hợp Lệ",
                             StatusCodes.Status400BadRequest);
                }
            }

            if (isCheckingTime == null || isCheckingTime == true)
            {
                var dayDoingExam = schedules[currentQuiz.NoSession - 1].Date;

                if (dayDoingExam.Date > GetCurrentTime().Date)
                {
                    throw new BadHttpRequestException($"Id [{examId}] Vẫn Chưa Tới Ngày Làm Bài Không Thể Chấm Điểm", StatusCodes.Status400BadRequest);
                }
            }

            var timeSpend = doingTime.Hour * 60 + doingTime.Minute;

            if (timeSpend <= 0 || timeSpend > 30)
            {
                throw new BadHttpRequestException($"Tổng Thời Gian Làm Không Hợp Lệ Vui Lòng Kiểm Tra Lại Yêu Cầu , [1-30] Phút", StatusCodes.Status400BadRequest);
            }
        }

        private async Task<Class> ValidateGradeQuizClass(Guid classId, Guid? currentStudentId)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == classId,
                include: x => x.Include(x => x.StudentClasses!).Include(x => x.Schedules.OrderBy(sc => sc.Date)));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            //if(cls.Status != ClassStatusEnum.PROGRESSING.ToString())
            //{
            //    throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            //}

            ValidateStudent(currentStudentId, cls, false);

            return cls;
        }

        private void ValidateStudent(Guid? studentId, Class cls, bool isOffLine)
        {
            var currentStudentClass = cls.StudentClasses.SingleOrDefault(sc => sc.StudentId == studentId);
            if (currentStudentClass == null)
            {
                string message = string.Empty;
                if (isOffLine)
                {
                    message = $"Id [{studentId}] Học Sinh Hiện Tại Không Thuộc Lớp Học Đang Cho Điểm Bài Tập";
                }
                else
                {
                    message = "Học Sinh Hiện Tại Không Thuộc Lớp Học Đang Tính Điểm Kiểm Tra";
                }
                throw new BadHttpRequestException(message, StatusCodes.Status400BadRequest);
            }

            if (currentStudentClass.SavedTime != null)
            {
                throw new BadHttpRequestException($"Id [{studentId}] Học Sinh Hiện Tại Thuộc Lớp Này, Đang Bảo Lưu Không Thể Thao Thực Hiện Các Thao Tác Liên Quan Đến Lớp Này", StatusCodes.Status400BadRequest);
            }
        }

        private async Task<Class> ValidateGradeExamOffLineClass(Guid classId, List<Guid> studentIdList)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == classId,
                include: x => x.Include(x => x.StudentClasses!).Include(x => x.Schedules.OrderBy(sc => sc.Date)));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }
            foreach (Guid id in studentIdList)
            {
                ValidateStudent(id, cls, true);
            }
            return cls;
        }

        public async Task<QuizResultResponse> GradeQuizFCAsync(QuizFCRequest quizStudentWork, TimeOnly doingTime, bool? isCheckingTime)
        {
            var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

            var cls = await ValidateGradeQuizClass(quizStudentWork.ClassId, currentStudentId);

            var quizzes = new List<QuestionPackage>();

            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == cls.CourseId,
                include: x => x.Include(x => x.ExamSyllabuses)!);

            var sessions = (await _unitOfWork.GetRepository<Topic>().GetListAsync(
                predicate: x => x.SyllabusId == syllabus.Id,
                include: x => x.Include(x => x.Sessions!))).SelectMany(x => x.Sessions!).ToList();

            foreach (var ses in sessions)
            {
                var package = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == ses.Id);
                if (package != null)
                {
                    quizzes.Add(package);
                }
            }

            var currentQuiz = quizzes.Find(q => q!.Id == quizStudentWork.ExamId);

            ValidateGradeCurrentQuiz(quizStudentWork.ExamId, currentQuiz, cls.Schedules.ToList(), doingTime, true, isCheckingTime);

            var currentTempQuiz = await _unitOfWork.GetRepository<TempQuiz>().SingleOrDefaultAsync(
                orderBy: x => x.OrderByDescending(x => x.CreatedTime),
                predicate: x => x.StudentId == currentStudentId && x.ExamId == currentQuiz!.Id,
                include: x => x.Include(x => x.Questions).ThenInclude(qt => qt.FCAnswers));

            if (currentTempQuiz == null)
            {
                throw new BadHttpRequestException($"Vui Lòng Truy Suất Câu Hỏi Trước Khi Tính Điểm", StatusCodes.Status400BadRequest);
            }

            if (currentTempQuiz.IsGraded == true)
            {
                throw new BadHttpRequestException($"Lần Làm Gần Nhất Gói Câu Hỏi Của Bài Kiểm Tra Đã Được Chấm Điểm Làm Vui Lòng Tuy Suất Gói Câu Hỏi Khác Và Làm Lại",
                StatusCodes.Status400BadRequest);
            }
            var numberAnswerStudentWorks = quizStudentWork.StudentQuestionResults.Sum(sq => sq.Answers.Count());
            ValidateTempQuizDB(numberAnswerStudentWorks, currentQuiz, currentTempQuiz, true);


            Guid testResultId;
            TestResult testResult;
            var examQuestions = new List<ExamQuestion>();
            var flashCardAnswers = new List<FlashCardAnswer>();
            int correctMark = 0;
            double scoreEarned = 0;
            string status = "Chưa Có Đánh Giá";

            int noAttempt = await GetAttempt(quizStudentWork.ExamId, currentStudentId, cls.Id, currentQuiz!.PackageType);

            GenrateTestResult(syllabus, currentQuiz, currentTempQuiz.TotalMark, cls.StudentClasses.ToList().Find(sc => sc.StudentId == currentStudentId)!, noAttempt, out testResultId, out testResult);
            var questionItems = ValidateStudentFCWorkRequest(quizStudentWork, currentTempQuiz);

            if (numberAnswerStudentWorks > 0)
            {
                foreach (var sqr in quizStudentWork.StudentQuestionResults)
                {
                    var currentQuestion = questionItems.Item1.Find(q => q.QuestionId == sqr.QuestionId);

                    var question = await _unitOfWork.GetRepository<Question>().SingleOrDefaultAsync(
                    predicate: x => x.Id == currentQuestion!.QuestionId,
                    include: x => x.Include(x => x.FlashCards!).ThenInclude(fc => fc.SideFlashCards));

                    var sideFlashCards = question.FlashCards!.SelectMany(fc => fc.SideFlashCards).ToList();

                    Guid examQuestionId = Guid.NewGuid();
                    examQuestions.Add(new ExamQuestion
                    {
                        Id = examQuestionId,
                        QuestionId = question.Id,
                        Question = question.Description,
                        QuestionImage = question.Img,
                        TestResultId = testResultId,
                    });

                    int wrongAttemps = 0;
                    foreach (var ar in sqr.Answers)
                    {
                        foreach (var fc in question.FlashCards!)
                        {
                            var currentFirstCardAnswer = fc.SideFlashCards.Find(sfc => sfc.Id == ar.FirstCardId);
                            if (currentFirstCardAnswer != null)
                            {
                                var currentSecondCardAnswer = sideFlashCards.Find(sfc => sfc.Id == ar.SecondCardId);
                                var correctSecondCard = fc.SideFlashCards.Find(sfc => sfc.Id != ar.FirstCardId);

                                bool isCorrect = false;
                                if (correctSecondCard!.Id == currentSecondCardAnswer!.Id)
                                {
                                    correctMark++;
                                    scoreEarned += fc.Score;
                                    isCorrect = true;
                                }

                                if (!isCorrect)
                                {
                                    wrongAttemps++;
                                    if (wrongAttemps > 3)
                                    {
                                        throw new BadHttpRequestException($"Yêu Cầu Không Hợp Lệ, Bài Làm Có Nhiều Hơn 3 Cặp Thẻ Bị Sai Vui Lòng Kiểm Tra Lại", StatusCodes.Status400BadRequest);
                                    }
                                }
                                GenerateFCResultItems(examQuestionId, flashCardAnswers, currentFirstCardAnswer, currentSecondCardAnswer,
                                correctSecondCard, isCorrect ? "Correct" : "Wrong", isCorrect ? fc.Score : 0);
                            }
                        }
                    }
                }
            }

            await GenerateFCResultNonAnswerIQuestions(questionItems.Item2, testResultId, examQuestions, flashCardAnswers);

            await GenerateFCResultNonAnswerFlashCards(examQuestions, flashCardAnswers, questionItems);

            var response = SettingLastResultInfor(doingTime, testResult, correctMark, scoreEarned, status, true);

            await SaveGrading(currentTempQuiz, testResult, examQuestions, null, flashCardAnswers);

            return response;
        }

        private QuizResultResponse SettingLastResultInfor(TimeOnly doingTime, TestResult testResult, int correctMark, double scoreEarned, string status, bool isFlashCard)
        {
            testResult.CorrectMark = correctMark;
            testResult.ScoreEarned = scoreEarned;
            testResult.ExamStatus = status;
            testResult.DoingTime = doingTime.ToTimeSpan();
            testResult.DoingDate = GetCurrentTime();

            var response = new QuizResultResponse
            {
                TotalMark = testResult.TotalMark,
                CorrectMark = isFlashCard ? (int)scoreEarned : correctMark,
                TotalScore = testResult.TotalScore,
                ScoreEarned = scoreEarned,
                DoingTime = doingTime.ToTimeSpan(),
                ExamStatus = status,
            };

            return response;
        }

        private async Task GenerateFCResultNonAnswerFlashCards(List<ExamQuestion> examQuestions, List<FlashCardAnswer> flashCardAnswers, (List<TempQuestion>, List<TempQuestion>, List<(List<TempFCAnswer>, Guid)>) questionItems)
        {
            if (questionItems.Item3 != null && questionItems.Item3.Count > 0)
            {
                var cardGenerated = new List<Guid>();

                foreach (var exam in examQuestions)
                {
                    var currentExamAnswerCards = questionItems.Item3.Find(item => item.Item2 == exam.QuestionId).Item1;
                    if (currentExamAnswerCards == null || !currentExamAnswerCards.Any())
                    {
                        continue;
                    }

                    foreach (var cea in currentExamAnswerCards)
                    {
                        var currentCoupleCardInfor = await _unitOfWork.GetRepository<FlashCard>().SingleOrDefaultAsync(
                            selector: x => x.SideFlashCards,
                            predicate: x => x.SideFlashCards.Any(sfc => sfc.Id == cea.CardId));

                        var flashCardAnswer = new FlashCardAnswer();
                        for (int i = 0; i < currentCoupleCardInfor.Count; i++)
                        {

                            if (cardGenerated.Any(id => id == currentCoupleCardInfor[i].Id))
                            {
                                continue;
                            }

                            cardGenerated.Add(currentCoupleCardInfor[i].Id);
                            if (i % 2 == 0)
                            {
                                flashCardAnswer.Id = Guid.NewGuid();
                                flashCardAnswer.LeftCardAnswerId = currentCoupleCardInfor[i]!.Id;
                                flashCardAnswer.LeftCardAnswer = currentCoupleCardInfor[i].Description;
                                flashCardAnswer.LeftCardAnswerImage = currentCoupleCardInfor[i].Image;
                                continue;
                            }

                            flashCardAnswer.CorrectRightCardAnswerId = currentCoupleCardInfor[i]!.Id;
                            flashCardAnswer.CorrectRightCardAnswer = currentCoupleCardInfor[i].Description;
                            flashCardAnswer.CorrectRightCardAnswerImage = currentCoupleCardInfor[i].Image;
                        }

                        flashCardAnswer.Score = 0;
                        flashCardAnswer.Status = "NotAnswer";
                        flashCardAnswer.ExamQuestionId = exam.Id;
                        flashCardAnswers.Add(flashCardAnswer);
                    }
                }
            }
        }

        private async Task GenerateFCResultNonAnswerIQuestions(List<TempQuestion>? nonAnswerTempQuestion, Guid testResultId, List<ExamQuestion> examQuestions, List<FlashCardAnswer> flashCardAnswers)
        {
            if (nonAnswerTempQuestion != null && nonAnswerTempQuestion.Any())
            {
                foreach (var tempQuestion in nonAnswerTempQuestion)
                {
                    var questionInfor = await _unitOfWork.GetRepository<Question>().SingleOrDefaultAsync(
                        predicate: x => x.Id == tempQuestion.QuestionId,
                        include: x => x.Include(x => x.FlashCards)!.ThenInclude(fc => fc.SideFlashCards)!);

                    var flashCardInfors = questionInfor.FlashCards!.SelectMany(fc => fc.SideFlashCards).ToList();

                    var TempflashCards = await _unitOfWork.GetRepository<TempQuestion>().SingleOrDefaultAsync(
                        selector: x => x.FCAnswers,
                        predicate: x => x.QuestionId == tempQuestion.QuestionId);

                    var groupedTempFlashCards = TempflashCards.GroupBy(x => x.NumberCoupleIdentify).Select(group => new
                    {
                        Identify = group.Key,
                        TempFlashCards = group.ToList()
                    }).ToList();

                    Guid examQuestionId = Guid.NewGuid();
                    examQuestions.Add(new ExamQuestion
                    {
                        Id = examQuestionId,
                        QuestionId = questionInfor.Id,
                        Question = questionInfor.Description,
                        QuestionImage = questionInfor.Img,
                        TestResultId = testResultId,
                    });

                    foreach (var group in groupedTempFlashCards)
                    {
                        var flasCardAnswer = new FlashCardAnswer();

                        for (int i = 0; i < group.TempFlashCards.Count; i++)
                        {
                            var currentCardInfor = flashCardInfors.Find(fci => fci.Id == group.TempFlashCards[i].CardId);

                            if (i % 2 == 0)
                            {
                                flasCardAnswer.Id = Guid.NewGuid();
                                flasCardAnswer.LeftCardAnswerId = currentCardInfor!.Id;
                                flasCardAnswer.LeftCardAnswer = currentCardInfor.Description;
                                flasCardAnswer.LeftCardAnswerImage = currentCardInfor.Image;
                                continue;
                            }

                            flasCardAnswer.CorrectRightCardAnswerId = currentCardInfor!.Id;
                            flasCardAnswer.CorrectRightCardAnswer = currentCardInfor.Description;
                            flasCardAnswer.CorrectRightCardAnswerImage = currentCardInfor.Image;
                        }

                        flasCardAnswer.Score = 0;
                        flasCardAnswer.Status = "NotAnswer";
                        flasCardAnswer.ExamQuestionId = examQuestionId;
                        flashCardAnswers.Add(flasCardAnswer);
                    }
                }
            }
        }

        public async Task<string> GradeExamOffLineAsync(ExamOffLineRequest exaOffLineStudentWork, bool? isCheckingTime)
        {
            string message = "Lưu Điểm Thành Công";

            var studentIdList = exaOffLineStudentWork.StudentQuizGardes.Select(sqg => sqg.StudentId).ToList();

            var cls = await ValidateGradeExamOffLineClass(exaOffLineStudentWork.ClassId, studentIdList);

            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == cls.CourseId,
                include: x => x.Include(x => x.ExamSyllabuses)!);

            var quizzes = new List<QuestionPackage>();

            var sessions = (await _unitOfWork.GetRepository<Topic>().GetListAsync(
                predicate: x => x.SyllabusId == syllabus.Id,
                include: x => x.Include(x => x.Sessions!))).SelectMany(x => x.Sessions!).ToList();

            foreach (var ses in sessions)
            {
                var package = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == ses.Id);
                if (package != null)
                {
                    quizzes.Add(package);
                }
            }

            var currentQuiz = quizzes.Find(q => q!.Id == exaOffLineStudentWork.ExamId);
            if (currentQuiz == null)
            {
                throw new BadHttpRequestException($"Bài Kiểm Tra Không Tồn Tại Hoặc Không Thuộc Lớp Đang Cho Điểm Bài Tập", StatusCodes.Status400BadRequest);
            }
            if (currentQuiz.Score != 0)
            {
                throw new BadHttpRequestException($"Bài Kiểm Tra Không Thuộc Dạng Tự Làm Tại Nhà, Yêu Cầu Không Hợp Lệ", StatusCodes.Status400BadRequest);
            }

            if (isCheckingTime == null || isCheckingTime == true)
            {
                var dayDoingExam = cls.Schedules.ToList()[currentQuiz.NoSession - 1].Date;
                if (dayDoingExam.Date > GetCurrentTime().Date)
                {
                    throw new BadHttpRequestException($"Id [{exaOffLineStudentWork.ExamId}] Vẫn Chưa Tới Ngày Nhập Điểm", StatusCodes.Status400BadRequest);
                }
            }


            var newTestResults = new List<TestResult>();
            var updateTestResults = new List<TestResult>();

            await GenerateTestOffLineResult(exaOffLineStudentWork, cls, syllabus, currentQuiz, newTestResults, updateTestResults);
            message = await SaveGradeRequest(message, newTestResults, updateTestResults);

            return message;
        }

        private async Task<string> SaveGradeRequest(string message, List<TestResult> newTestResults, List<TestResult> updateTestResults)
        {
            try
            {
                if (updateTestResults.Count() > 0)
                {
                    message += $", Các Học Sinh [{string.Join(", ", updateTestResults.Select(ur => ur.StudentClass!.StudentId))}] Đã Có Điểm Từ Trước Sẽ Được Cập Nhập Điểm Mới";
                    _unitOfWork.GetRepository<TestResult>().UpdateRange(updateTestResults);
                }
                if (newTestResults.Count() > 0)
                {
                    await _unitOfWork.GetRepository<TestResult>().InsertRangeAsync(newTestResults);
                }
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]" + ex.InnerException != null ? $"Inner: [{ex.InnerException}]" : "", StatusCodes.Status500InternalServerError);
            }

            return message;
        }

        private async Task GenerateTestOffLineResult(ExamOffLineRequest exaOffLineStudentWork, Class cls, Syllabus syllabus, QuestionPackage? currentQuiz, List<TestResult> newTestResults, List<TestResult> updateTestResults)
        {
            foreach (var studentWork in exaOffLineStudentWork.StudentQuizGardes)
            {
                var currentTest = await _unitOfWork.GetRepository<TestResult>().SingleOrDefaultAsync(
                    predicate: x => x.StudentClass!.StudentId == studentWork.StudentId && x.ExamId == currentQuiz!.Id && x.StudentClass.ClassId == cls.Id,
                    include: x => x.Include(x => x.StudentClass!));

                if (currentTest != null)
                {
                    currentTest.ScoreEarned = studentWork.Score;
                    currentTest.ExamStatus = studentWork.Status;
                    currentTest.DoingDate = GetCurrentTime();
                    updateTestResults.Add(currentTest);
                }
                else
                {
                    Guid testResultId;
                    TestResult testResult;
                    GenrateTestResult(syllabus, currentQuiz, 0, cls.StudentClasses.ToList().Find(sc => sc.StudentId == studentWork.StudentId)!, 1, out testResultId, out testResult);

                    testResult.TotalScore = 10;
                    testResult.ScoreEarned = studentWork.Score;
                    testResult.ExamStatus = studentWork.Status;
                    testResult.CorrectMark = (int)studentWork.Score;
                    testResult.DoingDate = GetCurrentTime();
                    newTestResults.Add(testResult);
                }
            }
        }

        public async Task<List<QuizResultExtraInforResponse>> GetCurrentStudentQuizDoneAsync()
        {
            var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

            var testResults = await _unitOfWork.GetRepository<TestResult>().GetListAsync(
                predicate: x => x.StudentClass!.StudentId == currentStudentId,
                include: x => x.Include(x => x.StudentClass!).Include(x => x.ExamQuestions));


            if (testResults == null && !testResults!.Any())
            {
                return new List<QuizResultExtraInforResponse>();
            }

            var responses = new List<QuizResultExtraInforResponse>();
            foreach (var test in testResults!)
            {
                //var studentWorks = new List<QuestionResultResponse>();

                //foreach (var examQuestion in test.ExamQuestions)
                //{
                //    var multipleChoiceAnswerResult = new MCAnswerResultResponse();
                //    //var flashCardAnswerResults = new List<FCAnswerResultResponse>();
                //    var flasCardAnswerResult = new FCAnswerResultResponse {};

                //    var multipleChoiceAnswer = await GetMCStudentResult(examQuestion, multipleChoiceAnswerResult);
                //    //var flashCardAnswers = await GetFCStudentResult(examQuestion, flashCardAnswerResults);

                //    studentWorks.Add(new QuestionResultResponse
                //    {
                //        QuestionId = examQuestion.QuestionId,
                //        QuestionDescription = examQuestion.Question,
                //        QuestionImage = examQuestion.QuestionImage,
                //        MultipleChoiceAnswerResult = multipleChoiceAnswer != null ? multipleChoiceAnswerResult : null,
                //        FlashCardAnswerResult = flashCardAnswers != null ? flashCardAnswerResults : null,
                //    });

                TimeSpan timeSpan = new TimeSpan(1, 25, 18); // 1 hour, 25 minutes, and 18 seconds

                // Create a DateTime object with the desired time
                DateTime dateTime = DateTime.Today.Add(timeSpan);

                // Extract the time portion from the DateTime object

                responses.Add(new QuizResultExtraInforResponse
                {
                    ResultId = test.Id,
                    ExamId = test.ExamId,
                    ExamName = test.ExamName,
                    QuizCategory = test.QuizCategory,
                    QuizType = test.QuizType,
                    QuizName = test.QuizName,
                    NoAttempt = test.NoAttempt,
                    TotalMark = test.TotalMark,
                    CorrectMark = test.CorrectMark,
                    TotalScore = test.TotalScore,
                    ScoreEarned = test.ScoreEarned,
                    ExamStatus = test.ExamStatus!,
                    DoingTime = test.DoingTime,
                    //StudentWorks = studentWorks,
                });
            }
            return responses;
        }

        private async Task<List<FCAnswerResultResponse>?> GetFCStudentResult(ExamQuestion examQuestion)
        {
            var flashCardAnswers = await _unitOfWork.GetRepository<FlashCardAnswer>().GetListAsync(predicate: x => x.ExamQuestionId == examQuestion.Id);

            if (flashCardAnswers != null)
            {
                var flashCardAnswerResults = new List<FCAnswerResultResponse>();
                foreach (var fc in flashCardAnswers)
                {
                    flashCardAnswerResults.Add(new FCAnswerResultResponse
                    {
                        StudentFirstCardAnswerId = fc.LeftCardAnswerId,
                        StudentFirstCardAnswerDecription = fc.LeftCardAnswer,
                        StudentFirstCardAnswerImage = fc.LeftCardAnswerImage,
                        StudentSecondCardAnswerId = fc.RightCardAnswerId,
                        StudentSecondCardAnswerDescription = fc.RightCardAnswer,
                        StudentSecondCardAnswerImage = fc.RightCardAnswerImage,
                        CorrectSecondCardAnswerId = fc.CorrectRightCardAnswerId,
                        CorrectSecondCardAnswerDescription = fc.CorrectRightCardAnswer,
                        CorrectSecondCardAnswerImage = fc.CorrectRightCardAnswerImage,
                        Status = fc.Status,
                        Score = fc.Score,
                    });
                }

                return flashCardAnswerResults;
            }

            return null;
        }

        private async Task<MCAnswerResultResponse?> GetMCStudentResult(ExamQuestion examQuestion)
        {
            var multipleChoiceAnswer = await _unitOfWork.GetRepository<MultipleChoiceAnswer>().SingleOrDefaultAsync(predicate: x => x.ExamQuestionId == examQuestion.Id);
            if (multipleChoiceAnswer != null)
            {
                var multipleChoiceAnswerResult = new MCAnswerResultResponse();

                multipleChoiceAnswerResult.StudentAnswerId = multipleChoiceAnswer.AnswerId == default ? null : multipleChoiceAnswer.AnswerId;
                multipleChoiceAnswerResult.StudentAnswerDescription = multipleChoiceAnswer.Answer;
                multipleChoiceAnswerResult.StudentAnswerImage = multipleChoiceAnswer.AnswerImage;
                multipleChoiceAnswerResult.CorrectAnswerId = multipleChoiceAnswer.CorrectAnswerId;
                multipleChoiceAnswerResult.CorrectAnswerDescription = multipleChoiceAnswer.CorrectAnswer;
                multipleChoiceAnswerResult.CorrectAnswerImage = multipleChoiceAnswer.CorrectAnswerImage;
                multipleChoiceAnswerResult.Status = multipleChoiceAnswer.Status;
                multipleChoiceAnswerResult.Score = multipleChoiceAnswer.Score;

                return multipleChoiceAnswerResult;
            }

            return null;
        }

        public async Task<List<FinalResultResponse>> GetFinalResultAsync(List<Guid> studentIdList)
        {
            var responses = new List<FinalResultResponse>();

            foreach (Guid studentId in studentIdList)
            {
                var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == studentId);
                if (student == null)
                {
                    throw new BadHttpRequestException($"Id [{studentId}] Của Học Sinh Không Tồn Tại", StatusCodes.Status400BadRequest);
                }

                if (!student.IsActive!.Value)
                {
                    throw new BadHttpRequestException($"Id [{studentId}] Của Học Sinh Đã Ngưng Hoạt Động", StatusCodes.Status400BadRequest);
                }

                var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                    predicate: x => x.StudentClasses.Any(sc => sc.StudentId == student.Id && sc.SavedTime == null) && x.Status != ClassStatusEnum.CANCELED.ToString(),
                    include: x => x.Include(x => x.Course!));

                if (classes == null || !classes.Any())
                {
                    throw new BadHttpRequestException($"Id [{studentId}] Của Học Sinh Chưa Tham Gia Bất Kỳ Lớp Học Của Khóa Học Nào", StatusCodes.Status400BadRequest);
                }

                await GenerateFinalResult(responses, student, classes);
            }
            return responses;
        }

        private async Task GenerateFinalResult(List<FinalResultResponse> responses, Student student, ICollection<Class> classes)
        {
            foreach (var cls in classes)
            {
                var schedules = (await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                    orderBy: x => x.OrderBy(x => x.Date),
                    predicate: x => x.ClassId == cls.Id)).ToList();

                if (schedules == null || !schedules.Any())
                {
                    throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh, Dữ Liệu Lịch Học Của Lớp Không Tồn Tại Vui Lòng Chờ Sử Lý", StatusCodes.Status500InternalServerError);
                }

                var identifyQuizExams = await GenerateIdentifyQuizExam(cls.CourseId);

                var finalResult = new FinalResultResponse
                {
                    ClassId = cls.Id,
                    CourseId = cls.CourseId,
                    StudentId = student.Id,
                    ClassName = cls.ClassCode,
                    CourseName = cls.Course!.Name,
                    StudentName = student!.FullName,
                };

                var allTestResult = (await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(
                    selector: x => x.TestResults,
                    predicate: x => x.ClassId == cls.Id && x.StudentId == student.Id)).ToList();

                var finalTestResults = new List<FinalTestResultResponse>();

                double participationWeight = 0.0, attendanceResult = 0.0, evaluateResult = 0.0;
                foreach (var quizExam in identifyQuizExams)
                {
                    if (quizExam.Item2 == null || quizExam.Item2 == default)
                    {
                        participationWeight = quizExam.Item1.Weight;
                        await CalculateParticipation(attendanceResult, evaluateResult, schedules, student.Id);
                    }
                    else
                    {
                        finalTestResults.Add(GenerateFinalTestResult(allTestResult, quizExam));
                    }
                }

                SettingLastResultInfor(finalResult, finalTestResults,
                (attendanceResult / schedules.Count) + (evaluateResult / schedules.Count), participationWeight);

                responses.Add(finalResult);
            }
        }

        private async Task CalculateParticipation(double attendanceResult, double evaluateResult, List<Schedule> schedules, Guid studentId)
        {
            foreach (var schedule in schedules)
            {
                var isPresent = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(
                    selector: x => x.IsPresent,
                    predicate: x => x.StudentId == studentId && x.ScheduleId == schedule.Id);

                var evaluate = await _unitOfWork.GetRepository<Evaluate>().SingleOrDefaultAsync(
                    selector: x => x.Status,
                    predicate: x => x.StudentId == studentId && x.ScheduleId == schedule.Id);

                attendanceResult += isPresent != null && isPresent.Value ? 10 : 0;

                if (evaluate != null)
                {
                    evaluateResult += evaluate == EvaluateStatusEnum.NOTGOOD.ToString() ? 5
                    : evaluate == EvaluateStatusEnum.NORMAL.ToString() ? 7 : 10;
                }
                else
                {
                    evaluateResult += 0;
                }
            }
        }
        private void SettingLastResultInfor(FinalResultResponse finalResult, List<FinalTestResultResponse> finalTestResults, double participationScore, double participationWeight)
        {
            var participationResult = new Participation
            {
                Weight = participationWeight,
                Score = participationScore,
                ScoreWeight = CalculateScoreWeight(participationWeight, participationScore),
            };

            var total = finalTestResults.Sum(ft => ft.ScoreWeight) + participationResult.ScoreWeight;
            string status = total >= 5 ? "Passed" : "Not Passed";
            if (finalTestResults.Any(ft => ft.Score == 0))
            {
                status = "Not Passed";
            }

            finalResult.Average = total;
            finalResult.Status = status;
            finalResult.QuizzesResults = finalTestResults;
            finalResult.ParticipationResult = participationResult;


        }

        private FinalTestResultResponse GenerateFinalTestResult(List<TestResult> allTestResult, (ExamSyllabus, QuestionPackage) quizExam)
        {
            var finalTestResult = new FinalTestResultResponse();

            var testResults = allTestResult.Where(tr => tr.ExamId == quizExam.Item2.Id).ToList();
            if (testResults is not null)
            {
                var testResult = testResults.OrderByDescending(x => x.NoAttempt).First();
                double weight = quizExam.Item1.Part == 2 ? quizExam.Item1.Weight / 2 : quizExam.Item1.Weight;

                finalTestResult.ExamId = quizExam.Item2.Id;
                finalTestResult.ExamName = "Bài Kiểm Tra Số" + quizExam.Item2.OrderPackage;
                finalTestResult.QuizName = quizExam.Item2.Title;
                finalTestResult.QuizType = quizExam.Item2.QuizType;
                finalTestResult.QuizCategory = quizExam.Item1.Category;
                finalTestResult.Weight = weight;
                finalTestResult.Score = testResult.ScoreEarned;
                finalTestResult.ScoreWeight = CalculateScoreWeight(weight, testResult.ScoreEarned);

            }
            return finalTestResult;
        }

        private async Task<List<(ExamSyllabus, QuestionPackage)>> GenerateIdentifyQuizExam(Guid courseId)
        {
            var identifyQuizExams = new List<(ExamSyllabus, QuestionPackage)>();

            var exams = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                selector: x => x.ExamSyllabuses,
                predicate: x => x.Course!.Id == courseId);

            var sessions = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                selector: x => x.Topics!.SelectMany(tp => tp.Sessions!),
                predicate: x => x.Course!.Id == courseId);

            foreach (var session in sessions)
            {
                var quiz = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(
                    predicate: x => x.SessionId == session.Id && x.QuizType.ToLower() != QuizTypeEnum.offline.ToString());

                if (quiz is not null && quiz.PackageType == PackageTypeEnum.ProgressTest.ToString())
                {
                    var examOfQuiz = exams!.ToList().Find(e => StringHelper.TrimStringAndNoSpace(e.ContentName!) == StringHelper.TrimStringAndNoSpace(quiz.ContentName!));

                    identifyQuizExams.Add(new(examOfQuiz!, quiz));
                }
            }
            var participationExam = exams!.First(e => StringHelper.TrimStringAndNoSpace(e.Category!).ToLower() == StringHelper.TrimStringAndNoSpace(PackageTypeEnum.Participation.ToString().ToLower()));
            identifyQuizExams.Add(new(participationExam, default!));

            return identifyQuizExams;
        }

        public double CalculateScoreWeight(double percentage, double score)
        {
            return (score * percentage) / 100;
        }

        public async Task<string> SettingExamTimeAsync(Guid examId, Guid classId, SettingQuizTimeRequest settingInfor)
        {
            await ValidateSettingRequest(examId, classId, settingInfor);
            try
            {
                var oldQuizTime = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(predicate: x => x.ClassId == classId && x.ExamId == examId);
                if (oldQuizTime != null)
                {
                    oldQuizTime.ExamStartTime = settingInfor.QuizStartTime != default ? settingInfor.QuizStartTime.ToTimeSpan() : oldQuizTime.ExamStartTime;
                    oldQuizTime.ExamEndTime = settingInfor.QuizEndTime != default ? settingInfor.QuizEndTime.ToTimeSpan() : oldQuizTime.ExamEndTime;
                    oldQuizTime.AttemptAllowed = settingInfor.AttemptAllowed!.Value;
                    oldQuizTime.Duration = settingInfor.Duration!.Value;

                    _unitOfWork.GetRepository<TempQuizTime>().UpdateAsync(oldQuizTime);
                    _unitOfWork.Commit();
                    return "Cập Nhập Thành Công";
                }
                var quizTime = new TempQuizTime
                {
                    Id = Guid.NewGuid(),
                    ClassId = classId,
                    ExamId = examId,
                    ExamStartTime = settingInfor.QuizStartTime.ToTimeSpan(),
                    ExamEndTime = settingInfor.QuizEndTime.ToTimeSpan(),
                    AttemptAllowed = settingInfor.AttemptAllowed!.Value,
                    Duration = settingInfor.Duration!.Value,
                };

                await _unitOfWork.GetRepository<TempQuizTime>().InsertAsync(quizTime);
                _unitOfWork.Commit();

                return "Thiết Lập Thành Công";
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]" + ex.InnerException != null ? $" InnerEx [{ex.InnerException}]" : string.Empty,
                StatusCodes.Status500InternalServerError);
            }

        }

        private async Task ValidateSettingRequest(Guid examId, Guid classId, SettingQuizTimeRequest settingInfor)
        {
            var courseId = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                     selector: x => x.CourseId,
                     predicate: x => x.Id == classId);

            if (courseId == default)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            var sessions = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                    selector: x => x.Topics!.SelectMany(tp => tp.Sessions!),
                    predicate: x => x.Course!.Id == courseId);

            bool isValid = false;

            var slot = new Slot { StartTime = default!, EndTime = default! };

            foreach (var session in sessions)
            {
                var quiz = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == session.Id);

                if (quiz != default && quiz.Id == examId)
                {
                    if (quiz.PackageType == PackageTypeEnum.ProgressTest.ToString() || quiz.PackageType == PackageTypeEnum.Review.ToString())
                    {
                        throw new BadHttpRequestException($"Bài Kiểm Tra Thuộc Dạng [{EnumUtil.CompareAndGetDescription<PackageTypeEnum>(quiz.PackageType)}, Không Yêu Cầu Thiết Lập Thời Gian]",
                              StatusCodes.Status400BadRequest);
                    }

                    var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                        orderBy: x => x.OrderBy(x => x.Date),
                        predicate: x => x.ClassId == classId,
                        include: x => x.Include(x => x.Slot)!);

                    slot = schedules.ToList()[session.NoSession - 1].Slot;
                    isValid = true;
                    break;
                }
            }

            if (!isValid)
            {
                throw new BadHttpRequestException($"Id [{examId}] Của Bài Kiểm Tra Không Tồn Tại Hoặc Không Thuộc Id Lớp Học Đang Yêu Cầu", StatusCodes.Status400BadRequest);
            }

            if (settingInfor.QuizStartTime == default && settingInfor.QuizEndTime == default)
            {
                return;
            }

            if (settingInfor.QuizStartTime > settingInfor.QuizEndTime)
            {
                throw new BadHttpRequestException($"Thiết Lập Không Hợp Lệ, Thời Gian Bắt Đầu Lớn Hơn Thời Gian Kết Thúc", StatusCodes.Status400BadRequest);
            }

            if (settingInfor.QuizStartTime < TimeOnly.Parse(slot!.StartTime))
            {
                throw new BadHttpRequestException($"Thiết Lập Không Hợp Lệ, Thời Gian Bắt Đầu Sớm Hơn Thời Gian Bắt Đầu {slot.StartTime} Của Buổi Học Có Bài Kiểm Tra", StatusCodes.Status400BadRequest);
            }

            if (settingInfor.QuizEndTime > TimeOnly.Parse(slot!.EndTime))
            {
                throw new BadHttpRequestException($"Thiết Lập Không Hợp Lệ, Thời Gian Kết Thúc Trễ Hơn Thời Gian Kết Thúc {slot.EndTime} Của Buổi Học Có Bài Kiểm Tra", StatusCodes.Status400BadRequest);
            }

            if (settingInfor.AttemptAllowed < 0 || settingInfor.AttemptAllowed > 10)
            {
                throw new BadHttpRequestException($"Số Lần Làm Quiz Không Hợp Lệ", StatusCodes.Status400BadRequest);
            }

            if (settingInfor.Duration < 0 || settingInfor.Duration < 60 || settingInfor.Duration > 18000)
            {
                throw new BadHttpRequestException($"Thời Gian Làm Quiz Không Hợp Lệ, Quá Ngắn Hoặc Quá Dài", StatusCodes.Status400BadRequest);
            }

        }

        public async Task<List<StudentWorkResult>> GetCurrentStudentQuizDoneWorkAsync(Guid examId, int? noAttempt)
        {
            var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

            var testResults = await _unitOfWork.GetRepository<TestResult>().GetListAsync(
                orderBy: x => x.OrderByDescending(x => x.NoAttempt),
                predicate: x => x.StudentClass!.StudentId == currentStudentId && x.ExamId == examId,
                include: x => x.Include(x => x.StudentClass!).Include(x => x.ExamQuestions));


            if (testResults == null || !testResults.Any())
            {
                throw new BadHttpRequestException($"Học Sinh Chưa Làm Bài Kiểm Tra Này ", StatusCodes.Status400BadRequest);
            }

            var currentTest = testResults.First();
            if (noAttempt != null)
            {
                currentTest = testResults.SingleOrDefault(tr => tr.NoAttempt == noAttempt);
                if (currentTest == null)
                {
                    throw new BadHttpRequestException($"Thứ Tự Lần Làm Không Hợp Lệ Vui Lòng Kiểm Tra Lại", StatusCodes.Status400BadRequest);
                }
            }

            var responses = new List<StudentWorkResult>();

            foreach (var examQuestion in currentTest.ExamQuestions)
            {
                var multipleChoiceAnswerResult = await GetMCStudentResult(examQuestion);
                var flashCardAnswerResults = await GetFCStudentResult(examQuestion);


                responses.Add(new StudentWorkResult
                {
                    QuestionId = examQuestion.QuestionId,
                    QuestionDescription = examQuestion.Question,
                    QuestionImage = examQuestion.QuestionImage,
                    MultipleChoiceAnswerResult = multipleChoiceAnswerResult != null ? multipleChoiceAnswerResult : null,
                    FlashCardAnswerResult = flashCardAnswerResults != null && flashCardAnswerResults.Any() ? flashCardAnswerResults : null,
                });
            }

            return responses;
        }

        public async Task<string> EvaluateExamOnLineAsync(Guid studentId, Guid examId, string status, int? noAttempt)
        {
            var testResults = await _unitOfWork.GetRepository<TestResult>().GetListAsync(
                orderBy: x => x.OrderByDescending(x => x.NoAttempt),
                predicate: x => x.StudentClass!.StudentId == studentId && x.ExamId == examId,
                include: x => x.Include(x => x.StudentClass!.Class)!);

            if (testResults is null || testResults.Count == 0)
            {
                throw new BadHttpRequestException($"Id Của Lớp/Học Sinh Không Tồn Tại, Hoặc Học Sinh Chưa Làm Bài Kiểm Tra Này", StatusCodes.Status400BadRequest);
            }

            if (GetRoleFromJwt() == RoleEnum.LECTURER.ToString())
            {
                if (testResults.Any(x => x.StudentClass!.Class!.LecturerId != GetUserIdFromJwt()))
                {
                    throw new BadHttpRequestException($"Bài Kiểm Tra Đang Đánh Giá Thuộc Lớp Không Phân Công Dạy Bởi Bạn", StatusCodes.Status400BadRequest);
                }
            }

            var testResult = testResults.ToList()[0];
            if (noAttempt != null)
            {
                testResult = testResults.SingleOrDefault(x => x.NoAttempt == noAttempt);
                if (testResult is null)
                {
                    throw new BadHttpRequestException($"Thứ Tự Lần Làm Kiểm Tra Không Hợp Lệ Vui Lòng Xem Lại", StatusCodes.Status400BadRequest);
                }
            }

            if (testResult!.QuizType == QuizTypeEnum.offline.ToString())
            {
                throw new BadHttpRequestException($"Bài Kiểm Tra Thuộc Dạng Tự Làm Tại Nhà Yêu Cầu Không Hợp Lệ", StatusCodes.Status400BadRequest);
            }

            try
            {
                testResult!.ExamStatus = status;
                _unitOfWork.GetRepository<TestResult>().UpdateAsync(testResult);
                _unitOfWork.Commit();

                return "Đánh Giá Bài Kiểm Tra Hoàn Tất";
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex}{ex.InnerException}]", StatusCodes.Status500InternalServerError);
            }

        }

        public async Task<List<StudenInforAndScore>> GetStudentInforAndScoreAsync(Guid classId, Guid? studentId)
        {
            var cls = await ValidateClass(classId, studentId);

            var quizzes = new List<QuestionPackage>();

            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == cls.CourseId,
                include: x => x.Include(x => x.ExamSyllabuses)!);

            var sessions = (await _unitOfWork.GetRepository<Topic>().GetListAsync(
                predicate: x => x.SyllabusId == syllabus.Id,
                include: x => x.Include(x => x.Sessions!))).SelectMany(x => x.Sessions!).ToList();

            foreach (var ses in sessions)
            {
                var package = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == ses.Id);
                if (package != null)
                {
                    quizzes.Add(package);
                }
            }

            var students = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(
                predicate: x => x.ClassId == classId,
                selector: x => x.Student);

            if (students == null || students.Count == 0)
            {
                throw new BadHttpRequestException($"Không Tìm Thấy Học Sinh Nào Trong Lớp", StatusCodes.Status400BadRequest);
            }

            if (studentId != null && studentId != default)
            {
                students = students.Where(s => s!.Id == studentId).ToList();
            }

            var exams = syllabus.ExamSyllabuses;
            var responses = new List<StudenInforAndScore>();
            await GenerateStudentInforAndScore(studentId, quizzes, students, exams, responses);

            return responses;
        }

        private async Task GenerateStudentInforAndScore(Guid? studentId, List<QuestionPackage> quizzes, ICollection<Student?> students, ICollection<ExamSyllabus>? exams, List<StudenInforAndScore> responses)
        {
            foreach (var stu in students)
            {
                var parent = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == stu!.ParentId);
                var studentWorkFullyInfors = new List<StudentWorkFullyInfor>();

                foreach (var quiz in quizzes)
                {
                    if(quiz.PackageType.ToLower() == PackageTypeEnum.Review.ToString().ToLower())
                    {
                        continue;
                    }

                    var testResult = await _unitOfWork.GetRepository<TestResult>().SingleOrDefaultAsync(
                        orderBy: x => x.OrderBy(x => x.NoAttempt),
                        predicate: x => x.StudentClass!.StudentId == studentId && x.ExamId == quiz.Id);

                    var exam = exams!.SingleOrDefault(e => StringHelper.TrimStringAndNoSpace(e.ContentName!) == StringHelper.TrimStringAndNoSpace(quiz.ContentName));

                    var extensionName = quiz.PackageType == PackageTypeEnum.FinalExam.ToString() ? "" : " " + quiz.OrderPackage;

                    if (testResult == null)
                    {
                        studentWorkFullyInfors.Add(new StudentWorkFullyInfor
                        {
                            ExamId = quiz.Id,
                            ExamName = "Bài " + quiz.ContentName.ToLower() + extensionName,
                            NoAttempt = 0,
                            QuizCategory = exam == null ? PackageTypeEnum.Review.ToString() : exam.Category!,
                            QuizType = quiz.QuizType.ToLower(),
                            QuizName = quiz.Score! == 0 ? "Làm Tại Lớp" : quiz.Title!,
                            TotalMark = null,
                            CorrectMark = null,
                            TotalScore = quiz.Score,
                            ScoreEarned = null,
                            DoingTime = null,
                            DoingDate = null,
                            ExamStatus = null,
                            Weight = null,
                        });
                    }
                    else
                    {
                        studentWorkFullyInfors.Add(new StudentWorkFullyInfor
                        {
                            ExamId = quiz.Id,
                            ExamName = "Bài " + quiz.ContentName.ToLower() + extensionName,
                            NoAttempt = testResult.NoAttempt,
                            QuizCategory = exam == null ? PackageTypeEnum.Review.ToString() : exam.Category!,
                            QuizType = quiz.QuizType.ToLower(),
                            QuizName = quiz.Score! == 0 ? "Làm Tại Lớp" : quiz.Title!,
                            TotalMark = testResult.TotalMark,
                            CorrectMark = testResult.CorrectMark,
                            TotalScore = quiz.Score,
                            ScoreEarned = testResult.ScoreEarned,
                            DoingTime = testResult.DoingTime,
                            DoingDate = testResult.DoingDate,
                            ExamStatus = testResult.ExamStatus,
                            Weight = exam != null ? exam.Weight : 0,
                        });
                    }
                }

                responses.Add(new StudenInforAndScore
                {
                    StudentInfor = _mapper.Map<StudentResponse>(stu),
                    ParentInfor = _mapper.Map<UserResponse>(parent),
                    ExamInfors = studentWorkFullyInfors,
                });
            }
        }
        #region UnUse Code

        //public async Task<List<FCQuizResponse>> GetFCQuestionPackageAsync(Guid examId)
        //{
        //    var fcQuestionPackage = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(
        //        predicate: x => x.Id == examId,
        //        include: x => x.Include(x => x.Questions!).ThenInclude(quest => quest.FlashCards)!.ThenInclude(fc => fc.SideFlashCards));

        //    if (fcQuestionPackage == null)
        //    {
        //        throw new BadHttpRequestException($"Id Của Bài Kiểm Tra Không Tồn Tại [{examId}]", StatusCodes.Status400BadRequest);
        //    }

        //    if (fcQuestionPackage.QuizType.ToLower() != QuizTypeEnum.flashcard.ToString())
        //    {
        //        throw new BadHttpRequestException($"Bài Kiểm Tra Không Thuộc Dạng Nối Thẻ Yêu Cầu Không Hợp Lệ", StatusCodes.Status400BadRequest);
        //    }

        //    var questions = fcQuestionPackage.Questions;
        //    if (questions == null || !questions.Any())
        //    {
        //        throw new BadHttpRequestException($"Lỗi Hệ Thống Bài Kiểm Tra Không Tìm Thấy Gói Câu Hỏi, Vui Lòng Chờ Nhân Viên Sử Lý", StatusCodes.Status500InternalServerError);
        //    }
        //    var responses = new List<FCQuizResponse>();
        //    foreach (var quest in questions)
        //    {
        //        var response = new FCQuizResponse
        //        {
        //            QuestionId = quest.Id,
        //            QuestionDescription = quest.Description,
        //            QuestionImage = quest.Img,
        //        };

        //        var fcAnswers = new List<CoupleFCAnswerResponse>();
        //        int numberCouple = 0;
        //        foreach (var fc in quest.FlashCards!)
        //        {
        //            numberCouple++;
        //            var coupleFcAnswer = new CoupleFCAnswerResponse();

        //            foreach (var sfc in fc.SideFlashCards)
        //            {
        //                coupleFcAnswer.CoupleFlashCard.Add(new FCAnswerResponse
        //                {
        //                    CardId = sfc.Id,
        //                    CardDescription = sfc.Description,
        //                    CardImage = sfc.Image,
        //                    NumberCoupleIdentify = numberCouple,
        //                    Score = fc.Score / 2,
        //                });
        //            }

        //            fcAnswers.Add(coupleFcAnswer);
        //        }

        //        response.CardAnswers = fcAnswers;
        //        responses.Add(response);
        //    }

        //    return responses;
        //}

        //public async Task<FullyExamRes> GetFullyExamInforStudent(Guid studentId, Guid examId)
        //{
        //    var response = new FullyExamRes();
        //    var testResult = await _unitOfWork.GetRepository<TestResult>().SingleOrDefaultAsync(
        //      predicate: x => x.StudentClass!.StudentId == studentId && x.ExamId == examId,
        //      include: x => x.Include(x => x.StudentClass!).Include(x => x.ExamQuestions));

        //    if (testResult != null && testResult.ExamType!.Trim().ToLower() != QuizTypeEnum.flashcard.ToString())
        //    {
        //        foreach (var examQuestion in testResult.ExamQuestions)
        //        {
        //            var multipleChoiceAnswerResult = new MCAnswerResultResponse();

        //            var multipleChoiceAnswer = await GetMCStudentResult(examQuestion, multipleChoiceAnswerResult);

        //            response.StudentWork!.Add(new StudentWorkResult
        //            {
        //                QuestionId = examQuestion.QuestionId,
        //                QuestionDescription = examQuestion.Question,
        //                QuestionImage = examQuestion.QuestionImage,
        //                MultipleChoiceAnswerResult = multipleChoiceAnswer != null ? multipleChoiceAnswerResult : null,
        //            });
        //        }
        //    }

        //}

        #endregion
    }
}
