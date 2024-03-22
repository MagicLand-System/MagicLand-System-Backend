using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain.Models.TempEntity.Class;
using MagicLand_System.Domain.Models.TempEntity.Quiz;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Request.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Result;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Final;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Linq;

namespace MagicLand_System.Services.Implements
{
    public class QuizService : BaseService<QuizService>, IQuizService
    {
        public QuizService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<QuizService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<QuizResultResponse> GradeQuizMCAsync(QuizMCRequest quizStudentWork)
        {
            var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

            Class cls = await ValidateGradeQuizClass(quizStudentWork.ClassId, currentStudentId);

            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.CourseId == cls.CourseId,
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

            ValidateGradeCurrentQuiz(quizStudentWork.ExamId, currentQuiz, false);

            var currentTempQuiz = await _unitOfWork.GetRepository<TempQuiz>().SingleOrDefaultAsync(
                orderBy: x => x.OrderByDescending(x => x.CreatedTime),
                predicate: x => x.StudentId == currentStudentId && x.ExamId == currentQuiz!.Id,
                include: x => x.Include(x => x.Questions).ThenInclude(qt => qt.MCAnswers));

            if (currentTempQuiz.IsGraded == true)
            {
                throw new BadHttpRequestException($"Gói Câu Hỏi Của Bài Kiểm Tra Đã Được Chấm Điểm Làm Vui Lòng Tuy Suất Gói Câu Hỏi Khác", StatusCodes.Status400BadRequest);
            }
            ValidateTempQuizDB(quizStudentWork.StudentQuestionResults.Count(), currentQuiz, currentTempQuiz);

            var questions = ValidateStudentMCWorkRequest(quizStudentWork, currentTempQuiz);

            int noAttempt = await GetAttempt(quizStudentWork.ExamId, currentStudentId, cls);

            Guid testResultId;
            TestResult testResult;
            GenrateTestResult(syllabus, currentQuiz, currentTempQuiz.TotalMark, cls.StudentClasses.ToList().Find(sc => sc.StudentId == currentStudentId)!, noAttempt, out testResultId, out testResult);

            var examQuestions = new List<ExamQuestion>();
            var multipleChoiceAnswers = new List<MultipleChoiceAnswer>();
            int correctMark = 0;
            double scoreEarned = 0;

            foreach (var sqr in quizStudentWork.StudentQuestionResults)
            {
                var currentQuestion = questions.Find(q => q.QuestionId == sqr.QuestionId);

                var question = await _unitOfWork.GetRepository<Question>().SingleOrDefaultAsync(
                predicate: x => x.Id == currentQuestion!.QuestionId,
                include: x => x.Include(x => x.MutipleChoices!));

                var currentAnswer = currentQuestion!.MCAnswers.ToList().Find(a => a.AnswerId == sqr.AnswerId);
                var currentCorrectAnswer = currentQuestion!.MCAnswers.ToList().Find(a => a.Score != 0);

                correctMark += currentAnswer!.Score != 0 ? +1 : 0;
                scoreEarned += currentAnswer!.Score != 0 ? currentAnswer.Score : 0;

                var answer = question.MutipleChoices!.Find(mc => mc.Id == currentAnswer!.AnswerId);
                var correctAnswer = question.MutipleChoices!.Find(mc => mc.Id == currentCorrectAnswer!.AnswerId);

                GenerateMCResultItems(testResultId, examQuestions, multipleChoiceAnswers, sqr, question, currentAnswer, answer, correctAnswer);
            }

            string status = GenerateExamStatus(testResult.TotalMark, correctMark);

            testResult.CorrectMark = correctMark;
            testResult.ScoreEarned = scoreEarned;
            testResult.ExamStatus = status;

            var response = new QuizResultResponse
            {
                TotalMark = testResult.TotalMark,
                CorrectMark = correctMark,
                TotalScore = testResult.TotalScore,
                ScoreEarned = scoreEarned,
                ExamStatus = status,
            };

            await SaveGrading(currentTempQuiz, testResult, examQuestions, multipleChoiceAnswers, null);

            return response;
        }

        private void GenerateMCResultItems(Guid testResultId, List<ExamQuestion> examQuestions, List<MultipleChoiceAnswer> multipleChoiceAnswers, MCStudentAnswer sqr, Question question, TempMCAnswer? currentAnswer, MultipleChoice? answer, MultipleChoice? correctAnswer)
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

                //if (flashCardAnswers != null)
                //{
                //    await _unitOfWork.GetRepository<FlashCardAnswer>().InsertRangeAsync(flashCardAnswers);
                //}

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

        private string GenerateExamStatus(int totalMark, int correctMark)
        {
            string status = string.Empty;
            double percentage = (double)correctMark / (double)totalMark;
            percentage = percentage * 100;

            if (percentage < 50)
            {
                status = "Not Good";
            }
            if (percentage >= 50 && percentage <= 70)
            {
                status = "Good";
            }
            if (percentage > 70)
            {
                status = "Excellent";
            }

            return status;
        }

        private void GenrateTestResult(Syllabus syllabus, QuestionPackage? currentQuiz, int totalMark, StudentClass studentClass, int noAttempt, out Guid testResultId, out TestResult testResult)
        {
            var currentExam = syllabus.ExamSyllabuses!.SingleOrDefault(es => es.ContentName!.Trim().ToLower() == currentQuiz!.ContentName!.Trim().ToLower());
            testResultId = Guid.NewGuid();
            testResult = new TestResult
            {
                Id = testResultId,
                ExamId = currentQuiz!.Id,
                ExamName = "Bài Kiểm Tra Số " + currentQuiz.OrderPackage,
                ExamCategory = currentExam != null ? currentExam.Category : QuizTypeEnum.review.ToString(),
                ExamType = currentQuiz.Type,
                TotalScore = currentQuiz.Score!.Value,
                TotalMark = totalMark,
                StudentClassId = studentClass.Id,
                NoAttempt = noAttempt,
            };
        }

        private async Task<int> GetAttempt(Guid examId, Guid? currentStudentId, Class cls)
        {
            int noAttempt = 1;
            var isExamHasDone = await _unitOfWork.GetRepository<TestResult>().GetListAsync(
                orderBy: x => x.OrderByDescending(x => x.NoAttempt),
                predicate: x => x.StudentClass!.StudentId == currentStudentId && x.StudentClass.ClassId == cls.Id && x.ExamId == examId);

            var quizTime = await _unitOfWork.GetRepository<TempQuizTime>().SingleOrDefaultAsync(
                predicate: x => x.ExamId == examId && x.ClassId == cls.Id);

            if (isExamHasDone != null && isExamHasDone.Any())
            {
                if (quizTime is null)
                {
                    throw new BadHttpRequestException($"Bạn Đã Làm Vượt Quá Số Lần Cho Phép Của Bài Kiểm Tra", StatusCodes.Status400BadRequest);
                }
                if (quizTime is not null && (isExamHasDone.Count() < quizTime.AttemptAllowed))
                {
                    noAttempt = isExamHasDone.First().NoAttempt++;
                }
                if (quizTime is not null && (isExamHasDone.Count() >= quizTime.AttemptAllowed))
                {
                    throw new BadHttpRequestException($"Bạn Đã Làm Vượt Quá Số Lần Cho Phép Của Bài Kiểm Tra", StatusCodes.Status400BadRequest);
                }
            }

            return noAttempt;
        }

        private List<TempQuestion> ValidateStudentMCWorkRequest(QuizMCRequest quizStudentWork, TempQuiz currentTempQuiz)
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

            return questions;
        }

        private static List<TempQuestion> ValidateStudentFCWorkRequest(QuizFCRequest quizStudentWork, TempQuiz currentTempQuiz)
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
                throw new BadHttpRequestException($"Một Số Id Câu Trả Lời Không Hợp Lệ Khi Không Thuộc Gói Câu Trả Lời Của Bài Kiểm Tra, [{string.Join(", ", invalidAnswer)}]",
                          StatusCodes.Status400BadRequest);
            }

            return questions;
        }

        private static void ValidateTempQuizDB(int totalMarkRequest, QuestionPackage? currentQuiz, TempQuiz currentTempQuiz)
        {
            if (currentTempQuiz == null)
            {
                throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh Không Thể Tìm Thấy Gói Câu Hỏi Của Bài Kiểm Tra, Vui Lòng Chờ Sử Lý",
                          StatusCodes.Status500InternalServerError);
            }

            if (currentTempQuiz.ExamType!.Trim().ToLower() != currentQuiz!.Type!.Trim().ToLower())
            {
                throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh Gói Câu Hỏi Không Thuộc Dạng Đề Của Bài Kiểm Tra, Vui Lòng Chờ Sử Lý",
                          StatusCodes.Status500InternalServerError);
            }

            if (totalMarkRequest != currentTempQuiz.TotalMark)
            {
                throw new BadHttpRequestException("Số Lượng Câu Hỏi Và Trả Lời Bài Làm Của Học Sinh Không Phù Hợp Với Số Lượng Câu Hỏi Và Câu Trả Lời Bộ Đề Của Bài Kiểm Tra",
                         StatusCodes.Status500InternalServerError);
            }
        }

        private static void ValidateGradeCurrentQuiz(Guid examId, QuestionPackage? currentQuiz, bool isFlashCard)
        {
            if (currentQuiz == null)
            {
                throw new BadHttpRequestException($"Id [{examId}] Bài Kiểm Tra Không Tồn Tại Hoặc Không Thuộc Lớp Đang Yêu Cầu Truy Vấn",
                          StatusCodes.Status400BadRequest);
            }

            if (currentQuiz.Score == 0)
            {
                throw new BadHttpRequestException($"Id [{examId}] Bài Kiểm Tra Thuộc Dạng Tự Làm Tại Nhà Cần Nhập Điểm Trực Tiếp",
                         StatusCodes.Status400BadRequest);
            }

            if (isFlashCard)
            {
                if (currentQuiz.Type != QuizTypeEnum.flashcard.ToString())
                {
                    throw new BadHttpRequestException($"Id [{examId}] Bài Kiểm Tra Thuộc Dạng Trắc Nghiệm, Yêu Cầu Không Hợp Lệ",
                             StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                if (currentQuiz.Type == QuizTypeEnum.flashcard.ToString())
                {
                    throw new BadHttpRequestException($"Id [{examId}] Bài Kiểm Tra Thuộc Dạng Nối Thẻ, Yêu Cầu Không Hợp Lệ",
                             StatusCodes.Status400BadRequest);
                }
            }

        }

        private async Task<Class> ValidateGradeQuizClass(Guid classId, Guid? currentStudentId)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == classId,
                include: x => x.Include(x => x.StudentClasses!));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại", StatusCodes.Status400BadRequest);
            }
            ValidateStudent(currentStudentId, cls, false);

            return cls;
        }

        private void ValidateStudent(Guid? studentId, Class cls, bool isOffLine)
        {
            if (!cls.StudentClasses.Any(sc => sc.StudentId == studentId))
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
        }

        private async Task<Class> ValidateGradeExamOffLineClass(Guid classId, List<Guid> studentIdList)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == classId,
                include: x => x.Include(x => x.StudentClasses!));

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

        public async Task<QuizResultResponse> GradeQuizFCAsync(Guid classId, Guid examId, double scoreEarned)
        {
            var currentStudentId = (await GetUserFromJwt()).StudentIdAccount;

            //Class cls = await ValidateGradeQuizClass(quizStudentWork.ClassId, currentStudentId);
            Class cls = await ValidateGradeQuizClass(classId, currentStudentId);

            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.CourseId == cls.CourseId,
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

            //var currentQuiz = quizzes.Find(q => q!.Id == quizStudentWork.ExamId);
            var currentQuiz = quizzes.Find(q => q!.Id == examId);

            //ValidateGradeCurrentQuiz(quizStudentWork.ExamId, currentQuiz, true);
            ValidateGradeCurrentQuiz(examId, currentQuiz, true);

            //var currentTempQuiz = await _unitOfWork.GetRepository<TempQuiz>().SingleOrDefaultAsync(
            //    orderBy: x => x.OrderByDescending(x => x.CreatedTime),
            //    predicate: x => x.StudentId == currentStudentId && x.ExamId == currentQuiz!.Id,
            //    include: x => x.Include(x => x.Questions).ThenInclude(qt => qt.FCAnswers));

            var currentTempQuiz = await _unitOfWork.GetRepository<TempQuiz>().SingleOrDefaultAsync(
              orderBy: x => x.OrderByDescending(x => x.CreatedTime),
              predicate: x => x.StudentId == currentStudentId && x.ExamId == currentQuiz!.Id);

            if(currentTempQuiz == null)
            {
                throw new BadHttpRequestException($"Vui Lòng Truy Suất Câu Hỏi Trước Khi Lưu Điểm", StatusCodes.Status400BadRequest);
            }
            //if (currentTempQuiz.IsGraded == true)
            //{
            //    throw new BadHttpRequestException($"Gói Câu Hỏi Của Bài Kiểm Tra Đã Được Chấm Điểm Làm Vui Lòng Tuy Suất Gói Câu Hỏi Khác Và Làm Lại", StatusCodes.Status400BadRequest);
            //}
            //ValidateTempQuizDB(quizStudentWork.StudentQuestionResults.Sum(sq => sq.Answers.Count()), currentQuiz, currentTempQuiz);

            //var questions = ValidateStudentFCWorkRequest(quizStudentWork, currentTempQuiz);

            //int noAttempt = await GetAttempt(quizStudentWork.ExamId, currentStudentId, cls);
            int noAttempt = await GetAttempt(examId, currentStudentId, cls);

            Guid testResultId;
            TestResult testResult;
            GenrateTestResult(syllabus, currentQuiz, currentTempQuiz.TotalMark, cls.StudentClasses.ToList().Find(sc => sc.StudentId == currentStudentId)!, noAttempt, out testResultId, out testResult);

            //var examQuestions = new List<ExamQuestion>();
            //var flashCardAnswers = new List<FlashCardAnswer>();
            //int correctMark = 0;
            //double scoreEarned = 0;

            //foreach (var sqr in quizStudentWork.StudentQuestionResults)
            //{
            //    var currentQuestion = questions.Find(q => q.QuestionId == sqr.QuestionId);

            //    var question = await _unitOfWork.GetRepository<Question>().SingleOrDefaultAsync(
            //    predicate: x => x.Id == currentQuestion!.QuestionId,
            //    include: x => x.Include(x => x.FlashCards!).ThenInclude(fc => fc.SideFlashCards));

            //    var sideFlashCards = question.FlashCards!.SelectMany(fc => fc.SideFlashCards).ToList();

            //    Guid examQuestionId = Guid.NewGuid();
            //    examQuestions.Add(new ExamQuestion
            //    {
            //        Id = examQuestionId,
            //        QuestionId = question.Id,
            //        Question = question.Description,
            //        QuestionImage = question.Img,
            //        TestResultId = testResultId,
            //    });

            //    foreach (var ar in sqr.Answers)
            //    {
            //        foreach (var fc in question.FlashCards!)
            //        {
            //            var currentFirstCardAnswer = fc.SideFlashCards.Find(sfc => sfc.Id == ar.FirstCardId);
            //            if (currentFirstCardAnswer != null)
            //            {
            //                var currentSecondCardAnswer = sideFlashCards.Find(sfc => sfc.Id == ar.SecondCardId);
            //                var correctSecondCard = fc.SideFlashCards.Find(sfc => sfc.Id != ar.FirstCardId);

            //                bool isCorrect = false;
            //                if (correctSecondCard!.Id == currentSecondCardAnswer!.Id)
            //                {
            //                    correctMark++;
            //                    scoreEarned += fc.Score;
            //                    isCorrect = true;
            //                }

            //                GenerateFCResultItems(examQuestionId, flashCardAnswers, currentFirstCardAnswer, currentSecondCardAnswer,
            //                correctSecondCard, isCorrect ? "Correct" : "Wrong", isCorrect ? fc.Score : 0);
            //            }
            //        }
            //    }
            //}
            string status = GenerateExamStatus(testResult.TotalMark, (int)scoreEarned);

            //testResult.CorrectMark = correctMark;
            //testResult.ScoreEarned = scoreEarned;
            //testResult.ExamStatus = status;

            testResult.CorrectMark = (int)scoreEarned;
            testResult.ScoreEarned = scoreEarned;
            var response = new QuizResultResponse
            {
                TotalMark = testResult.TotalMark,
                CorrectMark = (int)scoreEarned,
                TotalScore = testResult.TotalScore,
                ScoreEarned = scoreEarned,
                ExamStatus = status,
            };

            await SaveGrading(currentTempQuiz, testResult, null, null, null);

            return response;
        }

        public async Task<string> GradeExamOffLineAsync(ExamOffLineRequest exaOffLineStudentWork)
        {
            string message = "Lưu Điểm Thành Công";

            var studentIdList = exaOffLineStudentWork.StudentQuizGardes.Select(sqg => sqg.StudentId).ToList();

            Class cls = await ValidateGradeExamOffLineClass(exaOffLineStudentWork.ClassId, studentIdList);

            var syllabus = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.CourseId == cls.CourseId,
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
                    currentTest.ExamStatus = GenerateExamStatus(10, (int)studentWork.Score);
                    updateTestResults.Add(currentTest);
                }
                else
                {
                    Guid testResultId;
                    TestResult testResult;
                    GenrateTestResult(syllabus, currentQuiz, 0, cls.StudentClasses.ToList().Find(sc => sc.StudentId == studentWork.StudentId)!, 1, out testResultId, out testResult);

                    testResult.TotalScore = 10;
                    testResult.ScoreEarned = studentWork.Score;
                    testResult.ExamStatus = GenerateExamStatus(10, (int)studentWork.Score);
                    testResult.CorrectMark = 0;
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
                var studentWorks = new List<QuestionResultResponse>();

                foreach (var examQuestion in test.ExamQuestions)
                {
                    var multipleChoiceAnswerResult = new MCAnswerResultResponse();
                    var flashCardAnswerResults = new List<FCAnswerResultResponse>();

                    var multipleChoiceAnswer = await GetMCStudentResult(examQuestion, multipleChoiceAnswerResult);
                    var flashCardAnswers = await GetFCStudentResult(examQuestion, flashCardAnswerResults);

                    studentWorks.Add(new QuestionResultResponse
                    {
                        QuestionId = examQuestion.QuestionId,
                        QuestionDescription = examQuestion.Question,
                        QuestionImage = examQuestion.QuestionImage,
                        MultipleChoiceAnswerResult = multipleChoiceAnswer != null ? multipleChoiceAnswerResult : null,
                        FlashCardAnswerResults = flashCardAnswers != null ? flashCardAnswerResults : null,
                    });

                }

                responses.Add(new QuizResultExtraInforResponse
                {
                    ResultId = test.Id,
                    ExamId = test.ExamId,
                    ExamName = test.ExamName,
                    ExamCategory = test.ExamCategory,
                    ExamType = test.ExamType,
                    NoAttemp = test.NoAttempt,
                    TotalMark = test.TotalMark,
                    CorrectMark = test.CorrectMark,
                    TotalScore = test.TotalScore,
                    ScoreEarned = test.ScoreEarned,
                    ExamStatus = test.ExamStatus!,
                    StudentWorks = studentWorks,
                });
            }

            return responses;
        }

        private async Task<ICollection<FlashCardAnswer>?> GetFCStudentResult(ExamQuestion examQuestion, List<FCAnswerResultResponse> flashCardAnswerResults)
        {
            var flashCardAnswers = await _unitOfWork.GetRepository<FlashCardAnswer>().GetListAsync(predicate: x => x.ExamQuestionId == examQuestion.Id);
            if (flashCardAnswers != null)
            {
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
            }

            return flashCardAnswers;
        }

        private async Task<MultipleChoiceAnswer?> GetMCStudentResult(ExamQuestion examQuestion, MCAnswerResultResponse multipleChoiceAnswerResult)
        {
            var multipleChoiceAnswer = await _unitOfWork.GetRepository<MultipleChoiceAnswer>().SingleOrDefaultAsync(predicate: x => x.ExamQuestionId == examQuestion.Id);
            if (multipleChoiceAnswer != null)
            {
                multipleChoiceAnswerResult.StudentAnswerId = multipleChoiceAnswer.AnswerId;
                multipleChoiceAnswerResult.StudentAnswerDescription = multipleChoiceAnswer.Answer;
                multipleChoiceAnswerResult.StudentAnswerImage = multipleChoiceAnswer.AnswerImage;
                multipleChoiceAnswerResult.CorrectAnswerId = multipleChoiceAnswer.CorrectAnswerId;
                multipleChoiceAnswerResult.CorrectAnswerDescription = multipleChoiceAnswer.CorrectAnswer;
                multipleChoiceAnswerResult.CorrectAnswerImage = multipleChoiceAnswer.CorrectAnswerImage;
                multipleChoiceAnswer.Status = multipleChoiceAnswer.Status;
                multipleChoiceAnswer.Score = multipleChoiceAnswer.Score;
            }

            return multipleChoiceAnswer;
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
                    predicate: x => x.StudentClasses.Any(sc => sc.StudentId == student.Id) && x.Status != ClassStatusEnum.CANCELED.ToString(),
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
                        await CalculateParticipation(attendanceResult, evaluateResult, cls.Schedules.ToList(), student.Id);
                    }
                    else
                    {
                        finalTestResults.Add(GenerateFinalTestResult(allTestResult, quizExam));
                    }
                }

                SettingLastResultInfor(finalResult, finalTestResults,
                (attendanceResult / cls.Schedules.Count()) + (evaluateResult / cls.Schedules.Count()), participationWeight);

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
                finalTestResult.QuizName = "Bài Kiểm Tra Số" + quizExam.Item2.OrderPackage;
                finalTestResult.QuizType = quizExam.Item2.Type;
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
                predicate: x => x.CourseId == courseId);

            var sessions = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                selector: x => x.Topics!.SelectMany(tp => tp.Sessions!),
                predicate: x => x.CourseId == courseId);

            foreach (var session in sessions)
            {
                var quiz = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(
                    predicate: x => x.SessionId == session.Id && x.Type != QuizTypeEnum.options.ToString());

                if (quiz is not null)
                {
                    if (quiz.Type == QuizTypeEnum.review.ToString())
                    {
                        continue;
                    }
                    var examOfQuiz = exams!.ToList().Find(e => StringHelper.TrimStringAndNoSpace(e.ContentName!) == StringHelper.TrimStringAndNoSpace(quiz.ContentName!));

                    identifyQuizExams.Add(new(examOfQuiz!, quiz));
                }
            }
            var participationExam = exams!.First(e => StringHelper.TrimStringAndNoSpace(e.Category!) == StringHelper.TrimStringAndNoSpace(QuizTypeEnum.participation.ToString()));
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
                    oldQuizTime.ExamStartTime = settingInfor.QuizStartTime!.Value.ToTimeSpan();
                    oldQuizTime.ExamEndTime = settingInfor.QuizEndTime!.Value.ToTimeSpan();
                    oldQuizTime.AttemptAllowed = settingInfor.AttemptAllowed!.Value;

                    _unitOfWork.GetRepository<TempQuizTime>().UpdateAsync(oldQuizTime);
                    _unitOfWork.Commit();
                    return "Cập Nhập Thành Công";
                }
                var quizTime = new TempQuizTime
                {
                    Id = Guid.NewGuid(),
                    ClassId = classId,
                    ExamId = examId,
                    ExamStartTime = settingInfor.QuizStartTime!.Value.ToTimeSpan(),
                    ExamEndTime = settingInfor.QuizEndTime!.Value.ToTimeSpan(),
                    AttemptAllowed = settingInfor.AttemptAllowed!.Value,
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
                    selector: x => x.Topics!.SelectMany(tp => tp.Sessions!.Select(ses => ses.Id)),
                    predicate: x => x.CourseId == courseId);

            bool isValid = false;
            foreach (var id in sessions)
            {
                var quiz = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(
                    selector: x => x.Id,
                    predicate: x => x.SessionId == id);

                if (quiz != default && quiz == examId)
                {
                    isValid = true;
                    break;
                }
            }

            if (!isValid)
            {
                throw new BadHttpRequestException($"Id [{examId}] Của Bài Kiểm Tra Không Tồn Tại Hoặc Không Thuộc Id Lớp Học Đang Yêu Cầu", StatusCodes.Status400BadRequest);
            }

            if (settingInfor.AttemptAllowed < 0 || settingInfor.AttemptAllowed > 10)
            {
                throw new BadHttpRequestException($"Số Lần Làm Quiz Không Hợp Lệ", StatusCodes.Status400BadRequest);
            }
            if (settingInfor.QuizStartTime == default && settingInfor.QuizEndTime == default)
            {
                throw new BadHttpRequestException($"Thời Gian Cài Đặt Không Hợp Lệ", StatusCodes.Status400BadRequest);
            }
        }
    }
}
