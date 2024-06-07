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
using Quartz;

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
                    var isQuizDone = await _unitOfWork.GetRepository<ExamResult>().SingleOrDefaultAsync(
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
            var scheduleStartTime = schedule.Slot!.StartTime;
            var startTime = DateTime.Parse(date).Date.Add(TimeSpan.Parse(schedule.Slot!.StartTime));


            var addTime = 0;
            //if (scheduleStartTime.StartsWith("7"))
            //{
            //    addTime = 23 - 7;
            //}
            //if (scheduleStartTime.StartsWith("9"))
            //{
            //    addTime = 23 - 9;
            //}
            //if (scheduleStartTime.StartsWith("12"))
            //{
            //    addTime = 23 - 12;
            //}
            //if (scheduleStartTime.StartsWith("14"))
            //{
            //    addTime = 23 - 14;
            //}
            //if (scheduleStartTime.StartsWith("16"))
            //{
            //    addTime = 23 - 16;
            //}
            //if (scheduleStartTime.StartsWith("19"))
            //{
            //    addTime = 23 - 19;
            //}


            var endTime = DateTime.Parse(date).Date.Add(TimeSpan.Parse(schedule.Slot!.EndTime)).AddHours(addTime);
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
                include: x => x.Include(x => x.Course).Include(x => x.StudentClasses)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)!);

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id Lớp Học Không Hợp Lệ", StatusCodes.Status400BadRequest);
            }

            if (studentId != null && studentId != default)
            {
                if (cls.StudentClasses == null || !cls.StudentClasses.Any())
                {
                    throw new BadHttpRequestException($"Không Tìm Thấy Học Sinh Nào Trong Lớp", StatusCodes.Status400BadRequest);
                }

                var currentStudentClass = cls.StudentClasses.SingleOrDefault(sc => sc.StudentId == studentId.Value);

                if (currentStudentClass is null)
                {
                    throw new BadHttpRequestException($"Id [{studentId}] Của Học Sinh Không Thuộc Lớp Học Đang Truy Suất", StatusCodes.Status400BadRequest);
                }

                if (currentStudentClass.SavedTime != null)
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

                var test = await _unitOfWork.GetRepository<ExamResult>().SingleOrDefaultAsync(predicate: x => x.ExamId == exam.ExamId && x.StudentClass!.StudentId == studentId);
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

        public async Task<List<QuizResponse>> GetQuizOfExamtByExamIdAsync(Guid examId, Guid classId, int? examPart, bool isCheckingTime)
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

            await ValidateDayDoingExam(examId, cls, quiz, isCheckingTime);

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

            var currentStudent = await GetUserFromJwt();
            var studentClassId = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(selector: x => x.Id, predicate: x => x.ClassId == classId && x.StudentId == currentStudent.StudentIdAccount);

            await GenereateTempExam(examId, studentClassId, quiz, responses);
            //await GenereateTempExam(examId, quiz, responses);
            return responses;
        }

        private async Task ValidateDayDoingExam(Guid examId, Class cls, QuestionPackage quiz, bool isCheckingTime)
        {
            var sessions = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == cls.CourseId,
                selector: x => x.Topics!.SelectMany(tp => tp.Sessions!));

            if (!sessions.Any(ses => ses.Id == quiz.SessionId))
            {
                throw new BadHttpRequestException($"Id [{examId}] Của Bài Kiểm Tra Không Thuộc Lớp Đang Truy Suất", StatusCodes.Status400BadRequest);
            }

            if (isCheckingTime)
            {
                var dayDoingExam = cls.Schedules.ToList()[quiz.NoSession - 1].Date;

                if (dayDoingExam.Date > GetCurrentTime().Date)
                {
                    throw new BadHttpRequestException($"Id [{examId}] Của Bài Kiểm Tra Vẫn Chưa Tới Ngày Làm Bài Không Thể Truy Suất Câu Hỏi", StatusCodes.Status400BadRequest);
                }
            }
        }

        private async Task GenereateTempExam(Guid examId, Guid studentClassId, QuestionPackage exam, List<QuizResponse> responses)
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

                var tempQuestions = new List<ExamQuestion>();
                var tempMCAnswers = new List<MultipleChoiceAnswer>();
                var tempFCAnswers = new List<FlashCardAnswer>();

                Guid tempExamId = Guid.NewGuid();
                var tempExam = new ExamResult
                {
                    Id = tempExamId,
                    ExamId = examId,
                    StudentClassId = studentClassId,
                    TotalMark = totalMark,
                    TotalScore = exam.Score,
                    QuizType = exam.QuizType,
                    IsGraded = false,
                };
                foreach (var res in responses)
                {
                    var newTempQuestion = new ExamQuestion
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = res.QuestionId,
                        Question = res.QuestionDescription,
                        QuestionImage = res.QuestionImage,
                        ExamResultResultId = tempExamId,
                    };

                    tempQuestions.Add(newTempQuestion);

                    var multipleChoiceAnswers = res.AnswersMutipleChoicesInfor;
                    if (multipleChoiceAnswers != null && multipleChoiceAnswers.Count > 0)
                    {
                        foreach (var answer in multipleChoiceAnswers)
                        {
                            var newTempMCAnswer = new MultipleChoiceAnswer();
                            Guid mcAnswerId = Guid.NewGuid();
                            if (answer.Score != 0)
                            {
                                newTempMCAnswer.Id = mcAnswerId;
                                newTempMCAnswer.CorrectAnswerId = answer.AnswerId;
                                newTempMCAnswer.CorrectAnswer = answer.AnswerDescription;
                                newTempMCAnswer.CorrectAnswerImage = answer.AnswerImage;
                                newTempMCAnswer.Score = answer.Score;

                            }
                            else
                            {
                                continue;
                                //newTempMCAnswer.Id = mcAnswerId;
                                //newTempMCAnswer.AnswerId = answer.AnswerId;
                                //newTempMCAnswer.Answer = answer.AnswerDescription;
                                //newTempMCAnswer.AnswerImage = answer.AnswerImage;
                                //newTempMCAnswer.Score = 0;
                            }

                            newTempMCAnswer.ExamQuestionId = newTempQuestion.Id;
                            tempMCAnswers.Add(newTempMCAnswer);
                        }
                    }

                    var flashCardAnswers = res.AnwserFlashCarsInfor;
                    if (flashCardAnswers != null && flashCardAnswers.Count > 0)
                    {
                        var storedBrowserCard = new List<Guid>();

                        foreach (var firstCard in flashCardAnswers)
                        {
                            if (storedBrowserCard.Contains(firstCard.CardId))
                            {
                                continue;
                            }

                            var secondCard = flashCardAnswers.ToList().Single(fc => fc.CardId != firstCard.CardId && fc.NumberCoupleIdentify == firstCard.NumberCoupleIdentify);

                            tempFCAnswers.Add(new FlashCardAnswer
                            {
                                Id = Guid.NewGuid(),
                                LeftCardAnswerId = firstCard.CardId,
                                LeftCardAnswer = firstCard.CardDescription,
                                LeftCardAnswerImage = firstCard.CardImage,
                                RightCardAnswerId = secondCard!.CardId,
                                RightCardAnswer = secondCard.CardDescription,
                                RightCardAnswerImage = secondCard.CardImage,
                                Score = firstCard.Score + secondCard.Score,
                                ExamQuestionId = newTempQuestion.Id,
                            });

                            storedBrowserCard.Add(firstCard.CardId);
                            storedBrowserCard.Add(secondCard.CardId);
                        }
                    }
                }

                var existingTempExam = await _unitOfWork.GetRepository<ExamResult>().SingleOrDefaultAsync(
                    predicate: x => x.StudentClassId == studentClassId && x.ExamId == examId && x.IsGraded == false,
                    include: x => x.Include(x => x.ExamQuestions)!);

                if (existingTempExam != null)
                {
                    var examQuestionIds = existingTempExam.ExamQuestions.Select(eq => eq.Id).ToList();

                    var deleteMCTempAnswers = await _unitOfWork.GetRepository<MultipleChoiceAnswer>().GetListAsync(predicate: x => examQuestionIds.Contains(x.ExamQuestionId));

                    if (deleteMCTempAnswers.Any())
                    {
                        _unitOfWork.GetRepository<MultipleChoiceAnswer>().DeleteRangeAsync(deleteMCTempAnswers);
                    }

                    _unitOfWork.GetRepository<ExamResult>().DeleteAsync(existingTempExam);
                }

                await _unitOfWork.GetRepository<ExamResult>().InsertAsync(tempExam);
                await _unitOfWork.GetRepository<ExamQuestion>().InsertRangeAsync(tempQuestions);
                if (tempMCAnswers.Any())
                {
                    await _unitOfWork.GetRepository<MultipleChoiceAnswer>().InsertRangeAsync(tempMCAnswers);
                }
                if (tempFCAnswers.Any())
                {
                    await _unitOfWork.GetRepository<FlashCardAnswer>().InsertRangeAsync(tempFCAnswers);
                }
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status400BadRequest);
            }
        }
        #region old Save Temp 
        //private async Task GenereateTempExam(Guid examId, QuestionPackage quiz, List<QuizResponse> responses)
        //{
        //    try
        //    {
        //        int totalMark = 0;
        //        if (responses.SelectMany(r => r.AnwserFlashCarsInfor!).ToList().Any())
        //        {
        //            totalMark = responses.Sum(r => r.AnwserFlashCarsInfor!.Count()) / 2;
        //        }
        //        else
        //        {
        //            totalMark = responses.Count();
        //        }
        //        var tempQuestions = new List<TempQuestion>();
        //        var tempMCAnswers = new List<TempMCAnswer>();
        //        var tempFCAnswers = new List<TempFCAnswer>();

        //        Guid tempQuizId = Guid.NewGuid();
        //        var tempQuiz = new TempQuiz
        //        {
        //            Id = tempQuizId,
        //            ExamId = examId,
        //            StudentId = (await GetUserFromJwt()).StudentIdAccount!.Value,
        //            TotalMark = totalMark,
        //            ExamType = quiz.QuizType,
        //            CreatedTime = DateTime.Now,
        //            IsGraded = false,
        //        };
        //        foreach (var res in responses)
        //        {
        //            Guid tempQuestionId = Guid.NewGuid();
        //            tempQuestions.Add(new TempQuestion
        //            {
        //                Id = tempQuestionId,
        //                QuestionId = res.QuestionId,
        //                TempQuizId = tempQuizId,
        //            });

        //            var multipleChoiceAnswers = res.AnswersMutipleChoicesInfor;
        //            if (multipleChoiceAnswers != null && multipleChoiceAnswers.Count > 0)
        //            {
        //                foreach (var answer in multipleChoiceAnswers)
        //                {
        //                    tempMCAnswers.Add(new TempMCAnswer
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        AnswerId = answer.AnswerId,
        //                        Score = answer.Score,
        //                        TempQuestionId = tempQuestionId,
        //                    });
        //                }
        //            }
        //            var flashCardAnswers = res.AnwserFlashCarsInfor;
        //            if (flashCardAnswers != null && flashCardAnswers.Count > 0)
        //            {
        //                foreach (var answer in flashCardAnswers)
        //                {
        //                    tempFCAnswers.Add(new TempFCAnswer
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        CardId = answer.CardId,
        //                        Score = answer.Score,
        //                        NumberCoupleIdentify = answer.NumberCoupleIdentify,
        //                        TempQuestionId = tempQuestionId,
        //                    });
        //                }
        //            }
        //        }

        //        await _unitOfWork.GetRepository<TempQuiz>().InsertAsync(tempQuiz);
        //        await _unitOfWork.GetRepository<TempQuestion>().InsertRangeAsync(tempQuestions);
        //        if (tempMCAnswers.Any())
        //        {
        //            await _unitOfWork.GetRepository<TempMCAnswer>().InsertRangeAsync(tempMCAnswers);
        //        }
        //        if (tempFCAnswers.Any())
        //        {
        //            await _unitOfWork.GetRepository<TempFCAnswer>().InsertRangeAsync(tempFCAnswers);
        //        }
        //        _unitOfWork.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", StatusCodes.Status400BadRequest);
        //    }
        //}
        #endregion

        public async Task<QuizResultResponse> GradeQuizMCAsync(QuizMCRequest quizStudentWork, TimeOnly doingTime, bool? isCheckingTime)
        {
            var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

            var cls = await ValidateGradeQuizClass(quizStudentWork.ClassId, currentStudentId);

            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == cls.CourseId,
                include: x => x.Include(x => x.ExamSyllabuses)!);

            var exams = new List<QuestionPackage>();

            var sessions = (await _unitOfWork.GetRepository<Topic>().GetListAsync(
                predicate: x => x.SyllabusId == syllabus.Id,
                include: x => x.Include(x => x.Sessions!))).SelectMany(x => x.Sessions!).ToList();

            foreach (var ses in sessions)
            {
                var package = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == ses.Id);
                if (package != null)
                {
                    exams.Add(package);
                }
            }

            var currentExam = exams.Find(q => q!.Id == quizStudentWork.ExamId);

            ValidateGradeCurrentExam(quizStudentWork.ExamId, currentExam, cls.Schedules.ToList(), doingTime, false, isCheckingTime);

            var tempExam = await _unitOfWork.GetRepository<ExamResult>().SingleOrDefaultAsync(
                predicate: x => x.StudentClass!.StudentId == currentStudentId && x.ExamId == currentExam!.Id && x.NoAttempt == 0,
                include: x => x.Include(x => x.ExamQuestions));

            if (tempExam == null)
            {
                throw new BadHttpRequestException($"Bài Làm Không Hợp Lệ Khi Không Truy Suất Được Gói Câu Hỏi Trong Hệ Thống Hoặc Gói Câu Hỏi Đã Được Chấm, Vui Lòng Truy Suất Lại Gói Câu Hỏi Khác Và Làm Lại",
                    StatusCodes.Status400BadRequest);
            }

            if (tempExam.IsGraded == true)
            {
                throw new BadHttpRequestException($"Bài Làm Không Hợp Lệ Khi Gói Câu Hỏi Đã Được Chấm, Vui Lòng Truy Suất Lại Gói Câu Hỏi Khác Và Làm Lại",
                   StatusCodes.Status400BadRequest);
            }

            var currentStudentClass = await ValidateCurrentStudentClass(currentStudentId, cls);

            if (quizStudentWork.StudentQuestionResults.Count > 0)
            {
                ValidateTemExamDB(quizStudentWork.StudentQuestionResults.Count, currentExam, tempExam, false);
            }

            var examItems = await ValidateStudentMCWorkRequest(quizStudentWork, tempExam);
            int noAttempt = await GetAttempt(quizStudentWork.ExamId, currentStudentClass.StudentId, currentStudentClass.ClassId, currentExam!.PackageType);

            int correctMark = 0;
            double scoreEarned = 0;
            var questions = examItems.Item1;
            var multipleChoiceAnswers = examItems.Item2;
            var updateMCAnswers = new List<MultipleChoiceAnswer>();

            if (quizStudentWork.StudentQuestionResults.Count > 0)
            {
                foreach (var quest in questions)
                {
                    var answerQuestion = multipleChoiceAnswers.Single(a => a.Score != 0 && a.ExamQuestionId == quest.Id);
                    var studentWork = quizStudentWork.StudentQuestionResults.SingleOrDefault(sw => sw.QuestionId == quest.QuestionId);
                    if (studentWork == null)
                    {
                        answerQuestion.AnswerId = default;
                        answerQuestion.Answer = null;
                        answerQuestion.AnswerImage = null;
                        answerQuestion.Status = "NotAnswer";
                        answerQuestion.Score = 0;
                    }
                    else
                    {
                        var studentAnswer = multipleChoiceAnswers.Single(a => a.AnswerId == studentWork.AnswerId || a.CorrectAnswerId == studentWork.AnswerId);

                        correctMark += studentAnswer.Score != 0 ? +1 : 0;
                        scoreEarned += studentAnswer.Score != 0 ? studentAnswer.Score : 0;

                        answerQuestion.AnswerId = studentAnswer.Score != 0 ? studentAnswer.CorrectAnswerId : studentAnswer.AnswerId;
                        answerQuestion.Answer = studentAnswer.Score != 0 ? studentAnswer.CorrectAnswer : studentAnswer.Answer;
                        answerQuestion.AnswerImage = studentAnswer.Score != 0 ? studentAnswer.CorrectAnswerImage : studentAnswer.AnswerImage;
                        answerQuestion.Status = studentAnswer.Score != 0 ? "Correct" : "Wrong";
                        answerQuestion.Score = studentAnswer.Score != 0 ? answerQuestion.Score : 0;

                    }

                    quest.MultipleChoiceAnswer = answerQuestion;
                    updateMCAnswers.Add(answerQuestion);
                }
            }
            else
            {
                foreach (var quest in questions)
                {
                    var answerQuestion = multipleChoiceAnswers.Single(a => a.Score != 0 && a.ExamQuestionId == quest.Id);

                    answerQuestion.AnswerId = default;
                    answerQuestion.Answer = null;
                    answerQuestion.AnswerImage = null;
                    answerQuestion.Status = "NotAnswer";
                    answerQuestion.Score = 0;

                    quest.MultipleChoiceAnswer = answerQuestion;
                }
            }

            var examSyllabus = syllabus.ExamSyllabuses!.SingleOrDefault(es => es.ContentName!.Trim().ToLower() == currentExam!.ContentName!.Trim().ToLower());

            tempExam.CorrectMark = correctMark;
            tempExam.ScoreEarned = scoreEarned;
            tempExam.DoingTime = doingTime.ToTimeSpan();
            tempExam.IsGraded = true;
            tempExam.ExamName = "Bài Kiểm Tra Số " + currentExam.OrderPackage;
            tempExam.QuizCategory = examSyllabus != null ? examSyllabus.Category : PackageTypeEnum.Review.ToString();
            tempExam.QuizName = currentExam.Title;
            tempExam.NoAttempt = noAttempt;
            tempExam.ExamStatus = "Chưa Có Đánh Giá";


            var response = new QuizResultResponse
            {
                TotalMark = tempExam.TotalMark,
                CorrectMark = correctMark,
                TotalScore = tempExam.TotalScore,
                ScoreEarned = scoreEarned,
                DoingTime = doingTime.ToTimeSpan(),
                ExamStatus = "Chưa Có Đánh Giá",
            };

            try
            {
                _unitOfWork.GetRepository<ExamResult>().UpdateAsync(tempExam);
                _unitOfWork.GetRepository<MultipleChoiceAnswer>().UpdateRange(updateMCAnswers);
                _unitOfWork.Commit();

            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]" + ex.InnerException != null ? $"Inner: [{ex.InnerException}]" : "", StatusCodes.Status500InternalServerError);
            }

            return response;
        }

        #region old grade MC
        //public async Task<QuizResultResponse> GradeQuizMCAsync(QuizMCRequest quizStudentWork, TimeOnly doingTime, bool? isCheckingTime)
        //{
        //    var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

        //    var cls = await ValidateGradeQuizClass(quizStudentWork.ClassId, currentStudentId);

        //    var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
        //        predicate: x => x.Course!.Id == cls.CourseId,
        //        include: x => x.Include(x => x.ExamSyllabuses)!);

        //    var quizzes = new List<QuestionPackage>();

        //    var sessions = (await _unitOfWork.GetRepository<Topic>().GetListAsync(
        //        predicate: x => x.SyllabusId == syllabus.Id,
        //        include: x => x.Include(x => x.Sessions!))).SelectMany(x => x.Sessions!).ToList();

        //    foreach (var ses in sessions)
        //    {
        //        var package = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == ses.Id);
        //        if (package != null)
        //        {
        //            quizzes.Add(package);
        //        }
        //    }

        //    var currentQuiz = quizzes.Find(q => q!.Id == quizStudentWork.ExamId);

        //    ValidateGradeCurrentQuiz(quizStudentWork.ExamId, currentQuiz, cls.Schedules.ToList(), doingTime, false, isCheckingTime);

        //    var currentTempQuiz = await _unitOfWork.GetRepository<TempQuiz>().SingleOrDefaultAsync(
        //        orderBy: x => x.OrderByDescending(x => x.CreatedTime),
        //        predicate: x => x.StudentId == currentStudentId && x.ExamId == currentQuiz!.Id,
        //        include: x => x.Include(x => x.Questions).ThenInclude(qt => qt.MCAnswers));

        //    if (currentTempQuiz == null)
        //    {
        //        throw new BadHttpRequestException($"Bài Làm Không Hợp Lệ Khi Không Truy Suất Được Gói Câu Hỏi Trong Hệ Thống, Vui Lòng Truy Suất Lại Gói Câu Hỏi Và Làm Lại",
        //            StatusCodes.Status400BadRequest);
        //    }

        //    if (currentTempQuiz.IsGraded == true)
        //    {
        //        throw new BadHttpRequestException($"Gói Câu Hỏi Của Bài Kiểm Tra Đã Được Chấm Điểm Làm Vui Lòng Tuy Suất Gói Câu Hỏi Khác", StatusCodes.Status400BadRequest);
        //    }

        //    Guid testResultId;
        //    ExamResult testResult;

        //    var currentStudentClass = await ValidateCurrentStudentClass(currentStudentId, cls);

        //    if (quizStudentWork.StudentQuestionResults.Count > 0)
        //    {
        //        ValidateTempQuizDB(quizStudentWork.StudentQuestionResults.Count(), currentQuiz, currentTempQuiz, false);
        //    }

        //    var questionItems = ValidateStudentMCWorkRequest(quizStudentWork, currentTempQuiz);

        //    int noAttempt = await GetAttempt(quizStudentWork.ExamId, currentStudentClass.StudentId, currentStudentClass.ClassId, currentQuiz!.PackageType);

        //    GenrateTestResult(syllabus, currentQuiz, currentTempQuiz.TotalMark, currentStudentClass, noAttempt, out testResultId, out testResult);

        //    var examQuestions = new List<ExamQuestion>();
        //    var multipleChoiceAnswers = new List<MultipleChoiceAnswer>();
        //    int correctMark = 0;
        //    double scoreEarned = 0;
        //    string status = "Chưa Có Đánh Giá";

        //    if (quizStudentWork.StudentQuestionResults.Count > 0)
        //    {
        //        foreach (var sqr in quizStudentWork.StudentQuestionResults)
        //        {
        //            var currentTempQuestion = questionItems.Item1.Find(q => q.QuestionId == sqr.QuestionId);

        //            var currentQuestion = await _unitOfWork.GetRepository<Question>().SingleOrDefaultAsync(
        //            predicate: x => x.Id == currentTempQuestion!.QuestionId,
        //            include: x => x.Include(x => x.MutipleChoices!));

        //            var currentAnswer = currentTempQuestion!.MCAnswers.ToList().Find(a => a.AnswerId == sqr.AnswerId);
        //            var currentCorrectAnswer = currentTempQuestion!.MCAnswers.ToList().Find(a => a.Score != 0);

        //            correctMark += currentAnswer!.Score != 0 ? +1 : 0;
        //            scoreEarned += currentAnswer!.Score != 0 ? currentAnswer.Score : 0;

        //            var answer = currentQuestion.MutipleChoices!.Find(mc => mc.Id == currentAnswer!.AnswerId);
        //            var correctAnswer = currentQuestion.MutipleChoices!.Find(mc => mc.Id == currentCorrectAnswer!.AnswerId);

        //            GenerateMCResultItems(testResultId, examQuestions, multipleChoiceAnswers, sqr, currentQuestion, currentAnswer, answer, correctAnswer);
        //        }
        //    }


        //    await GenerateMCResultNonAnswerItems(questionItems.Item2, testResultId, examQuestions, multipleChoiceAnswers);

        //    var response = SettingLastResultInfor(doingTime, testResult, correctMark, scoreEarned, status, false);

        //    await SaveGrading(currentTempQuiz, testResult, examQuestions, multipleChoiceAnswers, null);

        //    return response;
        //}
        #endregion
        private async Task<StudentClass> ValidateCurrentStudentClass(Guid? currentStudentId, Class cls)
        {
            var currentStudent = cls.StudentClasses.SingleOrDefault(sc => sc.StudentId == currentStudentId);
            if (currentStudent == null)
            {
                var isMakeUpStudent = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(
                    predicate: x => x.Schedule!.ClassId == cls.Id && x.MakeUpFromScheduleId != null && x.StudentId == currentStudentId,
                    selector: x => x.MakeUpFromScheduleId);

                if (isMakeUpStudent == null)
                {
                    throw new BadHttpRequestException($"Id [{currentStudentId}] Của Học Sinh Không Hợp Lệ, Khi Không Thuộc Lớp Học Hiện Tại Hoặc Hệ Thống Không Tìm Thấy Lớp Đã Chuyển Lịch Học Bù", StatusCodes.Status400BadRequest);
                }

                currentStudent = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(predicate: x => x.Class!.Schedules.Any(sch => sch.Id == isMakeUpStudent && x.StudentId == currentStudentId));
                if (currentStudent == null)
                {
                    throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh Không Thể Tìm Thấy Lịch Học Phù Hợp Cho Id [{isMakeUpStudent}]", StatusCodes.Status400BadRequest);
                }
            }

            return currentStudent;
        }

        //private async Task GenerateMCResultNonAnswerItems(List<TempQuestion> nonAnswerTempQuestion, Guid testResultId, List<ExamQuestion> examQuestions, List<MultipleChoiceAnswer> multipleChoiceAnswers)
        //{
        //    if (nonAnswerTempQuestion.Any() && nonAnswerTempQuestion != null)
        //    {
        //        foreach (var tempQuestion in nonAnswerTempQuestion)
        //        {
        //            var currentNonAnswerQuestion = await _unitOfWork.GetRepository<Question>().SingleOrDefaultAsync(
        //                predicate: x => x.Id == tempQuestion.QuestionId,
        //                include: x => x.Include(x => x.MutipleChoices!));

        //            var currentCorrectAnswer = currentNonAnswerQuestion!.MutipleChoices!.ToList().Find(a => a.Score != 0);

        //            Guid examQuestionId = Guid.NewGuid();
        //            examQuestions.Add(new ExamQuestion
        //            {
        //                Id = examQuestionId,
        //                QuestionId = currentNonAnswerQuestion.Id,
        //                Question = currentNonAnswerQuestion.Description,
        //                QuestionImage = currentNonAnswerQuestion.Img,
        //                ExamResultResultId = testResultId,
        //            });

        //            multipleChoiceAnswers.Add(new MultipleChoiceAnswer
        //            {
        //                Id = Guid.NewGuid(),
        //                AnswerId = default,
        //                Answer = null,
        //                AnswerImage = null,
        //                CorrectAnswerId = currentCorrectAnswer!.Id,
        //                CorrectAnswer = currentCorrectAnswer.Description,
        //                CorrectAnswerImage = currentCorrectAnswer.Img,
        //                Status = "NotAnswer",
        //                Score = 0,
        //                ExamQuestionId = examQuestionId,
        //            });
        //        }
        //    }
        //}

        //private void GenerateMCResultItems(Guid testResultId, List<ExamQuestion> examQuestions, List<MultipleChoiceAnswer> multipleChoiceAnswers,
        //    MCStudentAnswer sqr, Question question, TempMCAnswer? currentAnswer, MultipleChoice? answer, MultipleChoice? correctAnswer)
        //{
        //    Guid examQuestionId = Guid.NewGuid();
        //    examQuestions.Add(new ExamQuestion
        //    {
        //        Id = examQuestionId,
        //        QuestionId = question.Id,
        //        Question = question.Description,
        //        QuestionImage = question.Img,
        //        ExamResultResultId = testResultId,
        //    });

        //    multipleChoiceAnswers.Add(new MultipleChoiceAnswer
        //    {
        //        Id = Guid.NewGuid(),
        //        AnswerId = sqr.AnswerId,
        //        Answer = answer!.Description,
        //        AnswerImage = answer.Img != null ? answer.Img : string.Empty,
        //        CorrectAnswerId = correctAnswer!.Id,
        //        CorrectAnswer = correctAnswer.Description,
        //        CorrectAnswerImage = correctAnswer.Img,
        //        Status = currentAnswer!.Score != 0 ? "Correct" : "Wrong",
        //        Score = currentAnswer.Score,
        //        ExamQuestionId = examQuestionId,
        //    });
        //}

        private void GenerateFCResultItems(Guid examQuestionId, List<FlashCardAnswer> flashCardAnswers,
            SideFlashCard firstCardAnswer, SideFlashCard secondCardAnswer, SideFlashCard correctSecondCardAnswer, string status, double score)
        {
            flashCardAnswers.Add(new FlashCardAnswer
            {
                Id = Guid.NewGuid(),
                LeftCardAnswerId = firstCardAnswer.Id,
                LeftCardAnswer = firstCardAnswer.Description,
                LeftCardAnswerImage = firstCardAnswer.Image,
                StudentCardAnswerId = secondCardAnswer.Id,
                StudentCardAnswer = secondCardAnswer.Description,
                StudentCardAnswerImage = secondCardAnswer.Image,
                RightCardAnswerId = correctSecondCardAnswer.Id,
                RightCardAnswer = correctSecondCardAnswer.Description,
                RightCardAnswerImage = correctSecondCardAnswer.Image,
                ExamQuestionId = examQuestionId,
                Status = status,
                Score = score,
            });
        }


        private async Task SaveGrading(TempQuiz currentTempQuiz, ExamResult testResult, List<ExamQuestion>? examQuestions, List<MultipleChoiceAnswer>? multipleChoiceAnswers, List<FlashCardAnswer>? flashCardAnswers)
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

                        //foreach (var eq in matchingQuestions)
                        //{
                        //    eq.MultipleChoiceAnswerId = mc.Id;
                        //}
                    }
                    await _unitOfWork.GetRepository<MultipleChoiceAnswer>().InsertRangeAsync(multipleChoiceAnswers);
                }

                if (flashCardAnswers != null)
                {
                    await _unitOfWork.GetRepository<FlashCardAnswer>().InsertRangeAsync(flashCardAnswers);
                }

                currentTempQuiz.IsGraded = true;
                _unitOfWork.GetRepository<TempQuiz>().UpdateAsync(currentTempQuiz);
                await _unitOfWork.GetRepository<ExamResult>().InsertAsync(testResult);
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

        private void UpdateExamResult(Syllabus syllabus, QuestionPackage? currentQuiz, int totalMark, StudentClass studentClass, int noAttempt, out Guid testResultId, out ExamResult testResult)
        {
            var currentExam = syllabus.ExamSyllabuses!.SingleOrDefault(es => es.ContentName!.Trim().ToLower() == currentQuiz!.ContentName!.Trim().ToLower());

            testResultId = Guid.NewGuid();
            testResult = new ExamResult
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

        private void GenrateExamOfflineResult(Syllabus syllabus, QuestionPackage? currentQuiz, int totalMark, StudentClass studentClass, int noAttempt, out Guid testResultId, out ExamResult testResult)
        {
            var currentExam = syllabus.ExamSyllabuses!.SingleOrDefault(es => es.ContentName!.Trim().ToLower() == currentQuiz!.ContentName!.Trim().ToLower());

            testResultId = Guid.NewGuid();
            testResult = new ExamResult
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
            var isExamHasDone = await _unitOfWork.GetRepository<ExamResult>().GetListAsync(
                orderBy: x => x.OrderByDescending(x => x.NoAttempt),
                predicate: x => x.StudentClass!.StudentId == currentStudentId && x.StudentClass.ClassId == classId && x.ExamId == examId && x.IsGraded == true);

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
        #region old get no attempt
        //private async Task<int> GetAttempt(Guid examId, Guid? currentStudentId, Guid classId, string packageType)
        //{
        //    int noAttempt = 1;
        //    var isExamHasDone = await _unitOfWork.GetRepository<ExamResult>().GetListAsync(
        //        orderBy: x => x.OrderByDescending(x => x.NoAttempt),
        //        predicate: x => x.StudentClass!.StudentId == currentStudentId && x.StudentClass.ClassId == classId && x.ExamId == examId);

        //    var quizTime = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(
        //        predicate: x => x.ExamId == examId && x.ClassId == classId);

        //    if (isExamHasDone != null && isExamHasDone.Any())
        //    {
        //        if (quizTime is null && packageType != PackageTypeEnum.Review.ToString())
        //        {
        //            throw new BadHttpRequestException($"Bạn Đã Làm Vượt Quá Số Lần Cho Phép Của Bài Kiểm Tra", StatusCodes.Status400BadRequest);
        //        }

        //        if (quizTime is null && packageType == PackageTypeEnum.Review.ToString())
        //        {
        //            noAttempt = isExamHasDone.First().NoAttempt + 1;
        //        }

        //        if (quizTime is not null && (isExamHasDone.Count() < quizTime.AttemptAllowed))
        //        {
        //            noAttempt = isExamHasDone.First().NoAttempt + 1;
        //        }
        //        if (quizTime is not null && (isExamHasDone.Count() >= quizTime.AttemptAllowed))
        //        {
        //            throw new BadHttpRequestException($"Bạn Đã Làm Đủ Số Lần Cho Phép Của Bài Kiểm Tra", StatusCodes.Status400BadRequest);
        //        }
        //    }

        //    return noAttempt;
        //}
        #endregion


        private async Task<(List<ExamQuestion>, List<MultipleChoiceAnswer>)> ValidateStudentMCWorkRequest(QuizMCRequest quizStudentWork, ExamResult tempExam)
        {
            var questionRequest = quizStudentWork.StudentQuestionResults.Select(sq => sq.QuestionId).ToList();
            var questions = tempExam.ExamQuestions.ToList();

            var invalidQuestion = questionRequest.Where(qr => !questions.Any(q => q.QuestionId == qr)).ToList();
            if (invalidQuestion != null && invalidQuestion.Any())
            {
                throw new BadHttpRequestException($"Một Số Id Câu Hỏi Không Hợp Lệ Khi Không Thuộc Gói Câu Hỏi Của Bài Kiểm Tra, [{string.Join(", ", invalidQuestion)}]",
                          StatusCodes.Status400BadRequest);
            }

            var questionIdList = questions.Select(q => q.Id).ToList();

            var answerRequest = quizStudentWork.StudentQuestionResults.Select(sq => sq.AnswerId).ToList();
            var answers = (await _unitOfWork.GetRepository<MultipleChoiceAnswer>().GetListAsync(predicate: x => questionIdList.Contains(x.ExamQuestionId))).ToList();
            var answersFromQP = (await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.Id == tempExam.ExamId, selector: x => x.Questions!.SelectMany(q => q.MutipleChoices!))).ToList();
            foreach (var afq in answersFromQP)
            {
                if (answers.Select(a => a.AnswerId).Contains(afq.Id) || answers.Select(a => a.CorrectAnswerId).Contains(afq.Id) || afq.Score != 0)
                {
                    continue;
                }
                answers.Add(new MultipleChoiceAnswer
                {
                    Id = default,
                    AnswerId = afq.Id,
                    Answer = afq.Description,
                    AnswerImage = afq.Img,
                });
            }


            var invalidAnswer = answerRequest.Where(ar => !answers.Any(a => a.AnswerId == ar && a.Score == 0 || a.CorrectAnswerId == ar)).ToList();
            if (invalidAnswer != null && invalidAnswer.Any())
            {
                throw new BadHttpRequestException($"Một Số Id Câu Trả Lời Không Hợp Lệ Khi Không Thuộc Gói Câu Trả Lời Của Bài Kiểm Tra, [{string.Join(", ", invalidAnswer)}]",
                          StatusCodes.Status400BadRequest);
            }

            return (questions, answers);
        }
        #region old validate student mc work 
        //private (List<TempQuestion>, List<TempQuestion>) ValidateStudentMCWorkRequest(QuizMCRequest quizStudentWork, TempQuiz currentTempQuiz)
        //{
        //    var questionRequest = quizStudentWork.StudentQuestionResults.Select(sq => sq.QuestionId).ToList();
        //    var questions = currentTempQuiz.Questions.ToList();


        //    var invalidQuestion = questionRequest.Where(qr => !questions.Any(q => q.QuestionId == qr)).ToList();
        //    if (invalidQuestion != null && invalidQuestion.Any())
        //    {
        //        throw new BadHttpRequestException($"Một Số Id Câu Hỏi Không Hợp Lệ Khi Không Thuộc Gói Câu Hỏi Của Bài Kiểm Tra, [{string.Join(", ", invalidQuestion)}]",
        //                  StatusCodes.Status400BadRequest);
        //    }

        //    var answerRequest = quizStudentWork.StudentQuestionResults.Select(sq => sq.AnswerId).ToList();
        //    var answers = questions.SelectMany(qt => qt.MCAnswers!).ToList();

        //    var invalidAnswer = answerRequest.Where(ar => !answers.Any(a => a.AnswerId == ar)).ToList();

        //    if (invalidAnswer != null && invalidAnswer.Any())
        //    {
        //        throw new BadHttpRequestException($"Một Số Id Câu Trả Lời Không Hợp Lệ Khi Không Thuộc Gói Câu Trả Lời Của Bài Kiểm Tra, [{string.Join(", ", invalidAnswer)}]",
        //                  StatusCodes.Status400BadRequest);
        //    }

        //    var questionNotAnswer = questions.Where(q => !questionRequest.Any(qr => qr == q.QuestionId)).ToList();

        //    return (questions, questionNotAnswer);
        //}
        #endregion


        private async Task<(List<ExamQuestion>, List<FlashCardAnswer>)> ValidateStudentFCWorkRequest(QuizFCRequest quizStudentWork, ExamResult tempExam)
        {
            var questionRequest = quizStudentWork.StudentQuestionResults.Select(sq => sq.QuestionId).ToList();
            var questions = tempExam.ExamQuestions.ToList();

            var invalidQuestion = questionRequest.Where(qr => !questions.Any(q => q.QuestionId == qr)).ToList();
            if (invalidQuestion != null && invalidQuestion.Any())
            {
                throw new BadHttpRequestException($"Một Số Id Câu Hỏi Không Hợp Lệ Khi Không Thuộc Gói Câu Hỏi Của Bài Kiểm Tra, [{string.Join(", ", invalidQuestion)}]",
                          StatusCodes.Status400BadRequest);
            }

            var questionIdList = questions.Select(q => q.Id).ToList();

            var answerRequest = quizStudentWork.StudentQuestionResults.SelectMany(sq => sq.Answers).ToList();
            var answers = (await _unitOfWork.GetRepository<FlashCardAnswer>().GetListAsync(predicate: x => questionIdList.Contains(x.ExamQuestionId))).ToList();

            var cardRequests = answerRequest.SelectMany(ar => new[] { ar.FirstCardId, ar.SecondCardId }).ToList();
            var cardAnswers = answers.SelectMany(a => new[] { a.LeftCardAnswerId, a.RightCardAnswerId }).ToHashSet();

            var invalidAnswer = cardRequests.Where(cr => !cardAnswers.Contains(cr)).ToList();

            if (invalidAnswer != null && invalidAnswer.Any())
            {
                throw new BadHttpRequestException($"Một Số Id Của Thẻ Trả Lời Không Hợp Lệ Khi Không Thuộc Gói Câu Hỏi Đã Truy Suất Gần Nhất Của Bài Kiểm Tra, [{string.Join(", ", invalidAnswer)}]",
                          StatusCodes.Status400BadRequest);
            }

            return (questions, answers);
        }
        #region old validate student fc work
        //private (List<TempQuestion>, List<TempQuestion>, List<(List<TempFCAnswer>, Guid)>) ValidateStudentFCWorkRequest(QuizFCRequest quizStudentWork, TempQuiz currentTempQuiz)
        //{
        //    var questionRequest = quizStudentWork.StudentQuestionResults.Select(sq => sq.QuestionId).ToList();
        //    var questions = currentTempQuiz.Questions.ToList();


        //    var invalidQuestion = questionRequest.Where(qr => !questions.Any(q => q.QuestionId == qr)).ToList();
        //    if (invalidQuestion != null && invalidQuestion.Any())
        //    {
        //        throw new BadHttpRequestException($"Một Số Id Câu Hỏi Không Hợp Lệ Khi Không Thuộc Gói Câu Hỏi Của Bài Kiểm Tra, [{string.Join(", ", invalidQuestion)}]",
        //                  StatusCodes.Status400BadRequest);
        //    }

        //    var answerRequest = quizStudentWork.StudentQuestionResults.SelectMany(sq => sq.Answers).ToList();
        //    var flashCards = new List<Guid>();
        //    foreach (var ar in answerRequest)
        //    {
        //        flashCards.Add(ar.FirstCardId);
        //        flashCards.Add(ar.SecondCardId);
        //    }

        //    var answers = questions.SelectMany(qt => qt.FCAnswers!).ToList();

        //    var invalidAnswer = flashCards.Where(fc => !answers.Any(a => a.CardId == fc)).ToList();

        //    if (invalidAnswer != null && invalidAnswer.Any())
        //    {
        //        throw new BadHttpRequestException($"Một Số Id Của Thẻ Trả Lời Không Hợp Lệ Khi Không Thuộc Gói Câu Hỏi Đã Truy Suất Gần Nhất Của Bài Kiểm Tra, [{string.Join(", ", invalidAnswer)}]",
        //                  StatusCodes.Status400BadRequest);
        //    }

        //    var questionNotAnswers = new List<TempQuestion>();
        //    var flashCardNotAnswers = new List<(List<TempFCAnswer>, Guid)>();

        //    foreach (var quest in questions)
        //    {
        //        //if (!questionRequest.Any(qr => qr == quest.QuestionId))
        //        //{

        //        //}
        //        var currentQuestRequest = quizStudentWork.StudentQuestionResults.Find(qr => qr.QuestionId == quest.QuestionId);
        //        if (currentQuestRequest == null)
        //        {
        //            questionNotAnswers.Add(quest);
        //            continue;
        //        }
        //        if (currentQuestRequest.Answers == null || !currentQuestRequest.Answers.Any())
        //        {
        //            questionNotAnswers.Add(quest);
        //            continue;
        //        }

        //        var allAnswerCurrentQuestion = currentTempQuiz.Questions.SelectMany(q => q.FCAnswers.Where(fc => fc.TempQuestionId == quest.Id)).ToList();
        //        var allCoupleCardAnswerRequest = quizStudentWork.StudentQuestionResults.Where(sqr => sqr.QuestionId == quest.QuestionId).SelectMany(sqr => sqr.Answers).ToList();
        //        var allCardAnswerRequest = new List<Guid>();

        //        foreach (var ar in allCoupleCardAnswerRequest)
        //        {
        //            allCardAnswerRequest.Add(ar.FirstCardId);
        //            allCardAnswerRequest.Add(ar.SecondCardId);
        //        }

        //        var cardNotAnswers = allAnswerCurrentQuestion.Where(aacq => !allCardAnswerRequest.Any(acar => acar == aacq.CardId)).ToList();

        //        if (cardNotAnswers.Any())
        //        {
        //            flashCardNotAnswers.Add((cardNotAnswers, quest.QuestionId));
        //        }
        //    }

        //    return (questions, questionNotAnswers, flashCardNotAnswers);
        //}
        #endregion
        private void ValidateTemExamDB(int totalQuestion, QuestionPackage? exam, ExamResult tempExam, bool isFlashCard)
        {

            if (tempExam.QuizType!.Trim().ToLower() != exam!.QuizType!.Trim().ToLower())
            {
                throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh Gói Câu Hỏi Không Thuộc Dạng Đề Của Bài Kiểm Tra, Vui Lòng Chờ Sử Lý",
                          StatusCodes.Status500InternalServerError);
            }

            if (!isFlashCard)
            {
                if (totalQuestion > tempExam.TotalMark)
                {
                    throw new BadHttpRequestException("Số Lượng Câu Hỏi Và Trả Lời Bài Làm Của Học Sinh Lớn Hơn Với Số Lượng Câu Hỏi Và Câu Trả Lời Bộ Đề Đã Truy Suất Gần Nhất Vui Lòng Xem Lại Bài Làm",
                             StatusCodes.Status400BadRequest);
                }
            }
        }

        #region old validate temp quiz db
        //private void ValidateTempQuizDB(int totalQuestion, QuestionPackage? currentQuiz, TempQuiz currentTempQuiz, bool isFlashCard)
        //{

        //    if (currentTempQuiz.ExamType!.Trim().ToLower() != currentQuiz!.QuizType!.Trim().ToLower())
        //    {
        //        throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh Gói Câu Hỏi Không Thuộc Dạng Đề Của Bài Kiểm Tra, Vui Lòng Chờ Sử Lý",
        //                  StatusCodes.Status500InternalServerError);
        //    }

        //    if (!isFlashCard)
        //    {
        //        if (totalQuestion > currentTempQuiz.TotalMark)
        //        {
        //            throw new BadHttpRequestException("Số Lượng Câu Hỏi Và Trả Lời Bài Làm Của Học Sinh Lớn Hơn Với Số Lượng Câu Hỏi Và Câu Trả Lời Bộ Đề Đã Truy Suất Gần Nhất Vui Lòng Xem Lại Bài Làm",
        //                     StatusCodes.Status400BadRequest);
        //        }
        //    }

        //}
        #endregion

        private void ValidateGradeCurrentExam(Guid examId, QuestionPackage? currentQuiz, List<Schedule> schedules, TimeOnly doingTime, bool isFlashCard, bool? isCheckingTime)
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
                include: x => x.Include(x => x.StudentClasses!).Include(x => x.Schedules.OrderBy(sch => sch.Date)));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }


            ValidateSavedStudent(currentStudentId, cls, false);

            return cls;
        }

        private void ValidateSavedStudent(Guid? studentId, Class cls, bool isOffLine)
        {
            var currentStudentClass = cls.StudentClasses.SingleOrDefault(sc => sc.StudentId == studentId);
            if (currentStudentClass == null)
            {
                return;
            }

            if (currentStudentClass.SavedTime != null)
            {
                throw new BadHttpRequestException($"Id [{studentId}] Học Sinh Hiện Tại Thuộc Lớp Này Hoặc Đang Bảo Lưu Không Thể Thực Hiện Các Thao Tác Liên Quan Đến Lớp Này", StatusCodes.Status400BadRequest);
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
                ValidateSavedStudent(id, cls, true);
            }
            return cls;
        }

        public async Task<QuizResultResponse> GradeQuizFCAsync(QuizFCRequest quizStudentWork, TimeOnly doingTime, bool? isCheckingTime)
        {
            var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

            var cls = await ValidateGradeQuizClass(quizStudentWork.ClassId, currentStudentId);

            var exams = new List<QuestionPackage>();

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
                    exams.Add(package);
                }
            }

            var currentExam = exams.Single(q => q!.Id == quizStudentWork.ExamId);

            ValidateGradeCurrentExam(quizStudentWork.ExamId, currentExam, cls.Schedules.ToList(), doingTime, true, isCheckingTime);

            var tempExam = await _unitOfWork.GetRepository<ExamResult>().SingleOrDefaultAsync(
                predicate: x => x.StudentClass!.StudentId == currentStudentId && x.ExamId == currentExam!.Id && x.IsGraded == false,
                include: x => x.Include(x => x.ExamQuestions));

            if (tempExam == null)
            {
                throw new BadHttpRequestException($"Bài Làm Không Hợp Lệ Khi Không Truy Suất Được Gói Câu Hỏi Trong Hệ Thống Hoặc Gói Câu Hỏi Đã Được Chấm, Vui Lòng Truy Suất Lại Gói Câu Hỏi Khác Và Làm Lại",
                    StatusCodes.Status400BadRequest);
            }

            var totalQuestionRequest = quizStudentWork.StudentQuestionResults.Count;
            ValidateTemExamDB(totalQuestionRequest, currentExam, tempExam, true);

            var currentStudentClass = await ValidateCurrentStudentClass(currentStudentId, cls);

            var examItems = await ValidateStudentFCWorkRequest(quizStudentWork, tempExam);
            int noAttempt = await GetAttempt(quizStudentWork.ExamId, currentStudentClass.StudentId, currentStudentClass.ClassId, currentExam!.PackageType);

            int correctMark = 0, wrongAttempt = 0;
            double scoreEarned = 0.0;
            var questions = examItems.Item1;
            var flashCardAnswers = examItems.Item2;
            var newFlashCardAnswers = new List<FlashCardAnswer>();

            if (totalQuestionRequest > 0)
            {
                var studentAnswerRequest = quizStudentWork.StudentQuestionResults.SelectMany(sq => sq.Answers).ToList();
                var browsedStudentAnswers = new List<FCStudentAnswer>();

                foreach (var quest in questions)
                {
                    var flashCardQuestionAnswers = flashCardAnswers.Where(fc => fc.ExamQuestionId == quest.Id).ToList();
                    foreach (var pairCard in flashCardQuestionAnswers)
                    {
                        var studentAnswers = studentAnswerRequest.Where(sa =>
                        pairCard.LeftCardAnswerId == sa.FirstCardId ||
                        pairCard.LeftCardAnswerId == sa.SecondCardId ||
                        pairCard.RightCardAnswerId == sa.FirstCardId ||
                        pairCard.RightCardAnswerId == sa.SecondCardId).ToList();

                        if (studentAnswers == null || !studentAnswers.Any())
                        {
                            pairCard.StudentCardAnswerId = default;
                            pairCard.StudentCardAnswer = null;
                            pairCard.StudentCardAnswerImage = null;
                            pairCard.Score = 0;
                            pairCard.Status = "NotAnswer";
                        }
                        else
                        {
                            foreach (var answer in studentAnswers)
                            {
                                if (browsedStudentAnswers.Contains(answer))
                                {
                                    continue;
                                }

                                browsedStudentAnswers.Add(answer);

                                if (wrongAttempt > 3)
                                {
                                    throw new BadHttpRequestException($"Bài Làm Không Hợp Lệ, Bài Làm Có Nhiều Hơn 3 Cặp Thẻ Bị Sai Vui Lòng Kiểm Tra Lại", StatusCodes.Status400BadRequest);
                                }

                                if (answer.FirstCardId == pairCard.LeftCardAnswerId)
                                {
                                    var result = await ValidateFCAsnwer(answer.SecondCardId, quest.Id, pairCard, newFlashCardAnswers, true);
                                    correctMark += result.Item1;
                                    scoreEarned += result.Item2;
                                    wrongAttempt += result.Item3;
                                    continue;
                                }
                                if (answer.FirstCardId == pairCard.RightCardAnswerId)
                                {
                                    var result = await ValidateFCAsnwer(answer.SecondCardId, quest.Id, pairCard, newFlashCardAnswers, false);
                                    correctMark += result.Item1;
                                    scoreEarned += result.Item2;
                                    wrongAttempt += result.Item3;
                                    continue;
                                }

                                if (answer.SecondCardId == pairCard.LeftCardAnswerId)
                                {
                                    var result = await ValidateFCAsnwer(answer.FirstCardId, quest.Id, pairCard, newFlashCardAnswers, true);
                                    correctMark += result.Item1;
                                    scoreEarned += result.Item2;
                                    wrongAttempt += result.Item3;
                                    continue;
                                }

                                if (answer.SecondCardId == pairCard.RightCardAnswerId)
                                {
                                    var result = await ValidateFCAsnwer(answer.FirstCardId, quest.Id, pairCard, newFlashCardAnswers, false);
                                    correctMark += result.Item1;
                                    scoreEarned += result.Item2;
                                    wrongAttempt += result.Item3;
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var pairCard in flashCardAnswers)
                {
                    pairCard.StudentCardAnswerId = default;
                    pairCard.StudentCardAnswer = null;
                    pairCard.StudentCardAnswerImage = null;
                    pairCard.Score = 0;
                    pairCard.Status = "NotAnswer";
                }
            }

            var examSyllabus = syllabus.ExamSyllabuses!.SingleOrDefault(es => es.ContentName!.Trim().ToLower() == currentExam!.ContentName!.Trim().ToLower());

            tempExam.CorrectMark = correctMark;
            tempExam.ScoreEarned = scoreEarned;
            tempExam.DoingTime = doingTime.ToTimeSpan();
            tempExam.IsGraded = true;
            tempExam.ExamName = "Bài Kiểm Tra Số " + currentExam.OrderPackage;
            tempExam.QuizCategory = examSyllabus != null ? examSyllabus.Category : PackageTypeEnum.Review.ToString();
            tempExam.QuizName = currentExam.Title;
            tempExam.NoAttempt = noAttempt;
            tempExam.ExamStatus = "Chưa Có Đánh Giá";


            var response = new QuizResultResponse
            {
                TotalMark = tempExam.TotalMark,
                CorrectMark = correctMark,
                TotalScore = tempExam.TotalScore,
                ScoreEarned = scoreEarned,
                DoingTime = doingTime.ToTimeSpan(),
                ExamStatus = "Chưa Có Đánh Giá",
            };

            try
            {
                var deleteCards = flashCardAnswers.Where(fc => string.IsNullOrEmpty(fc.Status)).ToList();
                var updateCards = flashCardAnswers.Where(fc => !string.IsNullOrEmpty(fc.Status)).ToList();

                if (deleteCards.Any())
                {
                    _unitOfWork.GetRepository<FlashCardAnswer>().DeleteRangeAsync(deleteCards);
                }
                _unitOfWork.GetRepository<ExamResult>().UpdateAsync(tempExam);
                _unitOfWork.GetRepository<FlashCardAnswer>().UpdateRange(updateCards);
                await _unitOfWork.GetRepository<FlashCardAnswer>().InsertRangeAsync(newFlashCardAnswers);
                _unitOfWork.Commit();

            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]" + ex.InnerException != null ? $"Inner: [{ex.InnerException}]" : "", StatusCodes.Status500InternalServerError);
            }


            return response;
        }

        private async Task<(int, double, int)> ValidateFCAsnwer(Guid answerOrderId, Guid examQuestionId, FlashCardAnswer pairCard, List<FlashCardAnswer> newFlashCardAnswers, bool isRightCard)
        {
            int correctMark = 0, wrongAttempt = 0;
            double scoreEarned = 0.0;

            var cardAnswerData = await _unitOfWork.GetRepository<SideFlashCard>().SingleOrDefaultAsync(predicate: x => x.Id == answerOrderId);

            if ((answerOrderId == pairCard.RightCardAnswerId && isRightCard) || (answerOrderId == pairCard.LeftCardAnswerId && !isRightCard))
            {

                pairCard.StudentCardAnswerId = cardAnswerData.Id;
                pairCard.StudentCardAnswer = cardAnswerData.Description;
                pairCard.StudentCardAnswerImage = cardAnswerData.Image;
                pairCard.Status = "Correct";
                correctMark++;
                scoreEarned = pairCard.Score;
            }
            else
            {
                newFlashCardAnswers.Add(new FlashCardAnswer
                {
                    Id = Guid.NewGuid(),
                    LeftCardAnswerId = isRightCard ? pairCard.LeftCardAnswerId : pairCard.RightCardAnswerId,
                    LeftCardAnswer = isRightCard ? pairCard.LeftCardAnswer : pairCard.RightCardAnswer,
                    LeftCardAnswerImage = isRightCard ? pairCard.LeftCardAnswer : pairCard.RightCardAnswerImage,
                    StudentCardAnswerId = cardAnswerData.Id,
                    StudentCardAnswer = cardAnswerData.Description,
                    StudentCardAnswerImage = cardAnswerData.Image,
                    RightCardAnswerId = isRightCard ? pairCard.RightCardAnswerId : pairCard.LeftCardAnswerId,
                    RightCardAnswer = isRightCard ? pairCard.RightCardAnswer : pairCard.LeftCardAnswer,
                    RightCardAnswerImage = isRightCard ? pairCard.RightCardAnswerImage : pairCard.LeftCardAnswerImage,
                    Status = "Wrong",
                    Score = 0,
                    ExamQuestionId = examQuestionId,
                });

                wrongAttempt++;
            }

            return (correctMark, scoreEarned, wrongAttempt);
        }

        #region old grade FC
        //public async Task<QuizResultResponse> GradeQuizFCAsync(QuizFCRequest quizStudentWork, TimeOnly doingTime, bool? isCheckingTime)
        //{
        //    var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

        //    var cls = await ValidateGradeQuizClass(quizStudentWork.ClassId, currentStudentId);

        //    var quizzes = new List<QuestionPackage>();

        //    var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
        //        predicate: x => x.Course!.Id == cls.CourseId,
        //        include: x => x.Include(x => x.ExamSyllabuses)!);

        //    var sessions = (await _unitOfWork.GetRepository<Topic>().GetListAsync(
        //        predicate: x => x.SyllabusId == syllabus.Id,
        //        include: x => x.Include(x => x.Sessions!))).SelectMany(x => x.Sessions!).ToList();

        //    foreach (var ses in sessions)
        //    {
        //        var package = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(predicate: x => x.SessionId == ses.Id);
        //        if (package != null)
        //        {
        //            quizzes.Add(package);
        //        }
        //    }

        //    var currentQuiz = quizzes.Find(q => q!.Id == quizStudentWork.ExamId);

        //    ValidateGradeCurrentQuiz(quizStudentWork.ExamId, currentQuiz, cls.Schedules.ToList(), doingTime, true, isCheckingTime);

        //    var currentTempQuiz = await _unitOfWork.GetRepository<TempQuiz>().SingleOrDefaultAsync(
        //        orderBy: x => x.OrderByDescending(x => x.CreatedTime),
        //        predicate: x => x.StudentId == currentStudentId && x.ExamId == currentQuiz!.Id,
        //        include: x => x.Include(x => x.Questions).ThenInclude(qt => qt.FCAnswers));

        //    if (currentTempQuiz == null)
        //    {
        //        throw new BadHttpRequestException($"Vui Lòng Truy Suất Câu Hỏi Trước Khi Tính Điểm", StatusCodes.Status400BadRequest);
        //    }

        //    if (currentTempQuiz.IsGraded == true)
        //    {
        //        throw new BadHttpRequestException($"Lần Làm Gần Nhất Gói Câu Hỏi Của Bài Kiểm Tra Đã Được Chấm Điểm Làm Vui Lòng Tuy Suất Gói Câu Hỏi Khác Và Làm Lại",
        //        StatusCodes.Status400BadRequest);
        //    }
        //    var numberAnswerStudentWorks = quizStudentWork.StudentQuestionResults.Sum(sq => sq.Answers.Count());
        //    ValidateTemExamDB(numberAnswerStudentWorks, currentQuiz, currentTempQuiz, true);


        //    Guid testResultId;
        //    ExamResult testResult;
        //    var examQuestions = new List<ExamQuestion>();
        //    var flashCardAnswers = new List<FlashCardAnswer>();
        //    int correctMark = 0;
        //    double scoreEarned = 0;
        //    string status = "Chưa Có Đánh Giá";

        //    var currentStudentClass = await ValidateCurrentStudentClass(currentStudentId, cls);

        //    int noAttempt = await GetAttempt(quizStudentWork.ExamId, currentStudentClass.StudentId, currentStudentClass.ClassId, currentQuiz!.PackageType);

        //    GenrateExamOfflineResult(syllabus, currentQuiz, currentTempQuiz.TotalMark, currentStudentClass, noAttempt, out testResultId, out testResult);
        //    var questionItems = ValidateStudentFCWorkRequest(quizStudentWork, currentTempQuiz);

        //    if (numberAnswerStudentWorks > 0)
        //    {
        //        foreach (var sqr in quizStudentWork.StudentQuestionResults)
        //        {
        //            var currentQuestion = questionItems.Item1.Find(q => q.QuestionId == sqr.QuestionId);

        //            var question = await _unitOfWork.GetRepository<Question>().SingleOrDefaultAsync(
        //            predicate: x => x.Id == currentQuestion!.QuestionId,
        //            include: x => x.Include(x => x.FlashCards!).ThenInclude(fc => fc.SideFlashCards));

        //            var sideFlashCards = question.FlashCards!.SelectMany(fc => fc.SideFlashCards).ToList();

        //            Guid examQuestionId = Guid.NewGuid();
        //            examQuestions.Add(new ExamQuestion
        //            {
        //                Id = examQuestionId,
        //                QuestionId = question.Id,
        //                Question = question.Description,
        //                QuestionImage = question.Img,
        //                ExamResultResultId = testResultId,
        //            });

        //            int wrongAttemps = 0;
        //            foreach (var ar in sqr.Answers)
        //            {
        //                foreach (var fc in question.FlashCards!)
        //                {
        //                    var currentFirstCardAnswer = fc.SideFlashCards.Find(sfc => sfc.Id == ar.FirstCardId);
        //                    if (currentFirstCardAnswer != null)
        //                    {
        //                        var currentSecondCardAnswer = sideFlashCards.Find(sfc => sfc.Id == ar.SecondCardId);
        //                        var correctSecondCard = fc.SideFlashCards.Find(sfc => sfc.Id != ar.FirstCardId);

        //                        bool isCorrect = false;
        //                        if (correctSecondCard!.Id == currentSecondCardAnswer!.Id)
        //                        {
        //                            correctMark++;
        //                            scoreEarned += fc.Score;
        //                            isCorrect = true;
        //                        }

        //                        if (!isCorrect)
        //                        {
        //                            wrongAttemps++;
        //                            if (wrongAttemps > 3)
        //                            {
        //                                throw new BadHttpRequestException($"Yêu Cầu Không Hợp Lệ, Bài Làm Có Nhiều Hơn 3 Cặp Thẻ Bị Sai Vui Lòng Kiểm Tra Lại", StatusCodes.Status400BadRequest);
        //                            }
        //                        }
        //                        GenerateFCResultItems(examQuestionId, flashCardAnswers, currentFirstCardAnswer, currentSecondCardAnswer,
        //                        correctSecondCard, isCorrect ? "Correct" : "Wrong", isCorrect ? fc.Score : 0);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    await GenerateFCResultNonAnswerIQuestions(questionItems.Item2, testResultId, examQuestions, flashCardAnswers);

        //    await GenerateFCResultNonAnswerFlashCards(examQuestions, flashCardAnswers, questionItems);

        //    var response = SettingLastResultInfor(doingTime, testResult, correctMark, scoreEarned, status, true);

        //    await SaveGrading(currentTempQuiz, testResult, examQuestions, null, flashCardAnswers);

        //    return response;
        //}
        #endregion

        private QuizResultResponse SettingLastResultInfor(TimeOnly doingTime, ExamResult testResult, int correctMark, double scoreEarned, string status, bool isFlashCard)
        {
            testResult.CorrectMark = correctMark;
            testResult.ScoreEarned = scoreEarned;
            testResult.ExamStatus = status;
            testResult.DoingTime = doingTime.ToTimeSpan();

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

                            flashCardAnswer.RightCardAnswerId = currentCoupleCardInfor[i]!.Id;
                            flashCardAnswer.RightCardAnswer = currentCoupleCardInfor[i].Description;
                            flashCardAnswer.RightCardAnswerImage = currentCoupleCardInfor[i].Image;
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
                        ExamResultResultId = testResultId,
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

                            flasCardAnswer.RightCardAnswerId = currentCardInfor!.Id;
                            flasCardAnswer.RightCardAnswer = currentCardInfor.Description;
                            flasCardAnswer.RightCardAnswerImage = currentCardInfor.Image;
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


            var newTestResults = new List<ExamResult>();
            var updateTestResults = new List<ExamResult>();

            await GenerateTestOffLineResult(exaOffLineStudentWork, cls, syllabus, currentQuiz, newTestResults, updateTestResults);
            message = await SaveGradeRequest(message, newTestResults, updateTestResults);

            return message;
        }

        private async Task<string> SaveGradeRequest(string message, List<ExamResult> newTestResults, List<ExamResult> updateTestResults)
        {
            try
            {
                if (updateTestResults.Count() > 0)
                {
                    updateTestResults.ForEach(t => t.IsGraded = true);
                    message += $", Các Học Sinh [{string.Join(", ", updateTestResults.Select(ur => ur.StudentClass!.StudentId))}] Đã Có Điểm Từ Trước Sẽ Được Cập Nhập Điểm Mới";
                    _unitOfWork.GetRepository<ExamResult>().UpdateRange(updateTestResults);
                }
                if (newTestResults.Count() > 0)
                {
                    newTestResults.ForEach(t => t.IsGraded = true);
                    await _unitOfWork.GetRepository<ExamResult>().InsertRangeAsync(newTestResults);
                }
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]" + ex.InnerException != null ? $"Inner: [{ex.InnerException}]" : "", StatusCodes.Status500InternalServerError);
            }

            return message;
        }

        private async Task GenerateTestOffLineResult(ExamOffLineRequest exaOffLineStudentWork, Class cls, Syllabus syllabus, QuestionPackage? currentQuiz, List<ExamResult> newTestResults, List<ExamResult> updateTestResults)
        {
            var checkingStudent = exaOffLineStudentWork.StudentQuizGardes.Select(x => x.StudentId).Where(id => !cls.StudentClasses.Select(sc => sc.StudentId).Contains(id)).ToList();

            foreach (var studentWork in exaOffLineStudentWork.StudentQuizGardes)
            {
                var currentStudentClass = new StudentClass();

                if (checkingStudent.Contains(studentWork.StudentId))
                {
                    var makeUpScheduleId = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(
                        selector: x => x.MakeUpFromScheduleId,
                        predicate: x => x.Schedule!.Class!.Id == cls.Id && x.MakeUpFromScheduleId != null);

                    if (makeUpScheduleId == null)
                    {
                        throw new BadHttpRequestException($"Id [{studentWork.StudentId}] Của Học Sinh Không Hợp Lệ, Khi Không Thuộc Lớp Đang Cho Điểm Hoặc Hệ Thống Không Tìm Thấy Lớp Gốc Của Học Sinh", StatusCodes.Status400BadRequest);
                    }

                    currentStudentClass = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(predicate: x => x.Class!.Schedules.Any(sch => sch.Id == makeUpScheduleId) && x.StudentId == studentWork.StudentId);
                }
                else
                {
                    currentStudentClass = cls.StudentClasses.SingleOrDefault(sc => sc.StudentId == studentWork.StudentId);
                }

                var currentTest = await _unitOfWork.GetRepository<ExamResult>().SingleOrDefaultAsync(
                    predicate: x => x.StudentClass!.StudentId == studentWork.StudentId && x.ExamId == currentQuiz!.Id && x.StudentClass.ClassId == currentStudentClass!.ClassId,
                    include: x => x.Include(x => x.StudentClass!));

                if (currentTest != null)
                {
                    currentTest.ScoreEarned = studentWork.Score;
                    currentTest.ExamStatus = studentWork.Status ?? currentTest.ExamStatus;
                    updateTestResults.Add(currentTest);
                }
                else
                {
                    Guid testResultId;
                    ExamResult testResult;
                    GenrateExamOfflineResult(syllabus, currentQuiz, 0, currentStudentClass!, 1, out testResultId, out testResult);

                    testResult.TotalScore = 10;
                    testResult.ScoreEarned = studentWork.Score;
                    testResult.ExamStatus = studentWork.Status ?? testResult.ExamStatus;
                    testResult.CorrectMark = (int)studentWork.Score;
                    newTestResults.Add(testResult);
                }
            }
        }

        public async Task<List<QuizResultExtraInforResponse>> GetCurrentStudentQuizDoneAsync()
        {
            var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

            var testResults = await _unitOfWork.GetRepository<ExamResult>().GetListAsync(
                predicate: x => x.StudentClass!.StudentId == currentStudentId && x.IsGraded == true,
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
                        StudentSecondCardAnswerId = fc.StudentCardAnswerId,
                        StudentSecondCardAnswerDescription = fc.StudentCardAnswer,
                        StudentSecondCardAnswerImage = fc.StudentCardAnswerImage,
                        CorrectSecondCardAnswerId = fc.RightCardAnswerId,
                        CorrectSecondCardAnswerDescription = fc.RightCardAnswer,
                        CorrectSecondCardAnswerImage = fc.RightCardAnswerImage,
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
                    selector: x => x.ExamResults,
                    predicate: x => x.ClassId == cls.Id && x.StudentId == student.Id)).ToList();

                var finalTestResults = new List<FinalTestResultResponse>();

                double participationWeight = 0.0, attendanceResult = 0.0, evaluateResult = 0.0;
                foreach (var quizExam in identifyQuizExams)
                {
                    if (quizExam.Item2 == null || quizExam.Item2 == default)
                    {
                        participationWeight = quizExam.Item1.Weight;
                        var partiResult = await CalculateParticipation(schedules, student.Id);
                        attendanceResult = partiResult.Item1;
                        evaluateResult = partiResult.Item2;
                    }
                    else
                    {
                        finalTestResults.Add(GenerateFinalTestResult(allTestResult, quizExam));
                    }
                }

                SettingLastResultInfor(finalResult, finalTestResults, identifyQuizExams,
                (attendanceResult + evaluateResult) / (schedules.Count * 2), participationWeight);

                responses.Add(finalResult);
            }
        }

        private async Task<(double, double)> CalculateParticipation(List<Schedule> schedules, Guid studentId)
        {
            double attendanceResult = 0.0, evaluateResult = 0.0;

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

            return (attendanceResult, evaluateResult);
        }
        private void SettingLastResultInfor(FinalResultResponse finalResult, List<FinalTestResultResponse> finalTestResults, List<(ExamSyllabus, QuestionPackage)> identifyQuizExams, double participationScore, double participationWeight)
        {
            var participationResult = new Participation
            {
                Weight = participationWeight,
                Score = participationScore,
                ScoreWeight = CalculateScoreWeight(participationWeight, participationScore),
            };

            var total = finalTestResults.Sum(ft => ft.ScoreWeight) + participationResult.ScoreWeight;
            string status = total >= 5 ? "Passed" : "Not Passed";
            //if (finalTestResults.Any(ft => ft.Score == 0))
            //{
            //    status = "Not Passed";
            //}

            foreach (var ftr in finalTestResults)
            {
                var quizExam = identifyQuizExams.SingleOrDefault(iqe => iqe.Item2.Id == ftr.ExamId);

                if (ftr.ScoreWeight < quizExam.Item1.CompletionCriteria)
                {
                    status = "Not Passed";
                    break;
                }
            }

            finalResult.Average = total;
            finalResult.Status = status;
            finalResult.QuizzesResults = finalTestResults;
            finalResult.ParticipationResult = participationResult;
        }

        private FinalTestResultResponse GenerateFinalTestResult(List<ExamResult> allTestResult, (ExamSyllabus, QuestionPackage) quizExam)
        {
            var finalTestResult = new FinalTestResultResponse();

            var testResults = allTestResult.Where(tr => tr.ExamId == quizExam.Item2.Id).ToList();

            double weight = quizExam.Item1.Part == 2 ? quizExam.Item1.Weight / 2 : quizExam.Item1.Weight;
            finalTestResult.ExamId = quizExam.Item2.Id;
            finalTestResult.ExamName = "Bài Kiểm Tra Số" + quizExam.Item2.OrderPackage;
            finalTestResult.QuizName = quizExam.Item2.Title;
            finalTestResult.QuizType = quizExam.Item2.QuizType;
            finalTestResult.QuizCategory = quizExam.Item1.Category;
            finalTestResult.Weight = weight;

            if (testResults is not null)
            {
                var testResult = testResults.OrderByDescending(x => x.NoAttempt).First();

                finalTestResult.Score = testResult.ScoreEarned;
                finalTestResult.ScoreWeight = CalculateScoreWeight(weight, testResult.ScoreEarned);

            }
            else
            {
                finalTestResult.Score = 0;
                finalTestResult.ScoreWeight = 0;
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

            var testResults = await _unitOfWork.GetRepository<ExamResult>().GetListAsync(
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
            var testResults = await _unitOfWork.GetRepository<ExamResult>().GetListAsync(
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
                _unitOfWork.GetRepository<ExamResult>().UpdateAsync(testResult);
                _unitOfWork.Commit();

                return "Đánh Giá Bài Kiểm Tra Hoàn Tất";
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex}{ex.InnerException}]", StatusCodes.Status500InternalServerError);
            }

        }

        public async Task<List<StudenInforAndScore>> GetStudentInforAndScoreAsync(Guid classId, Guid? studentId, Guid? examId)
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

            var studentClasses = cls.StudentClasses.ToList();
            var examSyllabuses = syllabus.ExamSyllabuses;


            if (studentId != null && studentId != default)
            {
                studentClasses = studentClasses.Where(sc => sc.StudentId == studentId).ToList();
            }

            if (examId != null && examId != default)
            {
                quizzes = quizzes.Where(quiz => quiz.Id == examId).ToList();
                if (quizzes == null || !quizzes.Any())
                {
                    throw new BadHttpRequestException($"Id [{examId}] Bài Kiểm Tra Không Hợp Lệ, Không Tồn Tại Hoạc Không Thuộc Lớp Đang Truy Vấn", StatusCodes.Status400BadRequest);
                }
            }

            var responses = new List<StudenInforAndScore>();
            await GenerateStudentInforAndScore(cls, quizzes, studentClasses, examSyllabuses, responses);

            return responses;
        }

        private async Task GenerateStudentInforAndScore(Class cls, List<QuestionPackage> quizzes, List<StudentClass> studentClasses, ICollection<ExamSyllabus>? exams, List<StudenInforAndScore> responses)
        {
            foreach (var sc in studentClasses)
            {
                var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == sc.StudentId, include: x => x.Include(x => x.Parent)!);
                var studentWorkFullyInfors = new List<StudentWorkFullyInfor>();

                foreach (var quiz in quizzes)
                {
                    if (quiz.PackageType.ToLower() == PackageTypeEnum.Review.ToString().ToLower())
                    {
                        continue;
                    }

                    var allTestResult = await _unitOfWork.GetRepository<ExamResult>().GetListAsync(
                        orderBy: x => x.OrderByDescending(x => x.NoAttempt),
                        predicate: x => x.StudentClass!.StudentId == sc.StudentId && x.ExamId == quiz.Id && x.IsGraded == true);

                    var currentExam = exams!.SingleOrDefault(e => StringHelper.TrimStringAndNoSpace(e.ContentName!) == StringHelper.TrimStringAndNoSpace(quiz.ContentName));

                    var extensionName = quiz.PackageType == PackageTypeEnum.FinalExam.ToString() ? "" : " " + quiz.OrderPackage;

                    var currentWeight = currentExam != null ? currentExam.Weight / currentExam.Part : 0;
                    if (allTestResult == null || !allTestResult.Any())
                    {
                        studentWorkFullyInfors.Add(new StudentWorkFullyInfor
                        {
                            ExamId = quiz.Id,
                            ExamName = "Bài " + quiz.ContentName.ToLower() + extensionName,
                            NoAttempt = 0,
                            QuizCategory = currentExam == null ? PackageTypeEnum.Review.ToString() : currentExam.Category!,
                            QuizType = quiz.QuizType.ToLower(),
                            QuizName = quiz.Score! == 0 ? "Làm Tại Lớp" : quiz.Title!,
                            TotalMark = null,
                            CorrectMark = null,
                            TotalScore = quiz.Score,
                            ScoreEarned = cls.Status == ClassStatusEnum.COMPLETED.ToString() ? 0 : null,
                            DoingTime = null,
                            DoingDate = cls.Schedules.ToList()[quiz.NoSession - 1].Date,
                            ExamStatus = null,
                            Weight = currentWeight,
                        });
                    }
                    else
                    {
                        var testResult = allTestResult.First();

                        studentWorkFullyInfors.Add(new StudentWorkFullyInfor
                        {
                            ExamId = quiz.Id,
                            ExamName = "Bài " + quiz.ContentName.ToLower() + extensionName,
                            NoAttempt = testResult.NoAttempt,
                            QuizCategory = currentExam == null ? PackageTypeEnum.Review.ToString() : currentExam.Category!,
                            QuizType = quiz.QuizType.ToLower(),
                            QuizName = quiz.Score! == 0 ? "Làm Tại Lớp" : quiz.Title!,
                            TotalMark = testResult.TotalMark,
                            CorrectMark = testResult.CorrectMark,
                            TotalScore = quiz.Score,
                            ScoreEarned = testResult.ScoreEarned,
                            DoingTime = testResult.DoingTime,
                            DoingDate = cls.Schedules.ToList()[quiz.NoSession - 1].Date,
                            ExamStatus = testResult.ExamStatus,
                            Weight = currentWeight,
                        });
                    }
                }
                double participationScore = 0;
                var participationWeight = exams!.Single(e => e.Category!.Trim().ToLower() == "participation").Weight;

                if (cls.Status == ClassStatusEnum.COMPLETED.ToString())
                {
                    var partiResult = await CalculateParticipation(cls.Schedules.ToList(), sc.StudentId);

                    participationScore = (partiResult.Item1 + partiResult.Item2) / (cls.Schedules.Count * 2);
                }

                responses.Add(new StudenInforAndScore
                {
                    StudentInfor = _mapper.Map<StudentResponse>(student),
                    ParentInfor = _mapper.Map<UserResponse>(student.Parent),
                    ExamInfors = studentWorkFullyInfors,
                    ParticipationInfor = new Participation
                    {
                        Score = participationScore,
                        Weight = participationWeight,
                        ScoreWeight = CalculateScoreWeight(participationWeight, participationScore),
                    },
                });
            }
        }

    }
}