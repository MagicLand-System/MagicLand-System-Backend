using MagicLand_System.Constants;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Quizzes;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Custom;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Answers;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
using MagicLand_System.PayLoad.Response.Quizzes.Result;
using MagicLand_System_Web_Dev.Pages.DataContants;
using MagicLand_System_Web_Dev.Pages.Enums;
using MagicLand_System_Web_Dev.Pages.Helper;
using MagicLand_System_Web_Dev.Pages.Message.SubMessage;
using MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage;
using MagicLand_System_Web_Dev.Pages.Messages.InforMessage;
using MagicLand_System_Web_Dev.Pages.Messages.SubMessage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace MagicLand_System_Web_Dev.Pages
{
    public class TakeQuizModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public TakeQuizModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public bool IsLoading { get; set; }

        [BindProperty]
        public List<ClassDefaultMessage> Classes { get; set; } = new List<ClassDefaultMessage>();

        [BindProperty]
        public StudentQuizInforMessage CurrentStudentQuizInforMessage { get; set; } = null;


        public async Task<IActionResult> OnGet()
        {
            IsLoading = false;
            var data = SessionHelper.GetObjectFromJson<List<StudentQuizInforMessage>>(HttpContext!.Session, "DataQuiz");
            var classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext!.Session, "Classes");

            if (data != null && data.Count > 0)
            {
                CurrentStudentQuizInforMessage = data.First();
                ViewData["IndexPage"] = 0;
            }

            if (classes != null && classes.Count > 0)
            {
                Classes = classes;
            }
            else
            {
                await FetchClass();

                return Page();
            }

            return Page();
        }

        private async Task FetchClass()
        {
            var result = await _apiHelper.FetchApiAsync<List<ClassWithSlotShorten>>(ApiEndpointConstant.ClassEnpoint.GetAll, MethodEnum.GET, null);

            if (result.Data == null)
            {
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Classes", Classes);
            }
            else
            {
                var classes = new List<ClassDefaultMessage>();
                var classFiltered = result.Data.Where(x => x.Status == ClassStatusEnum.PROGRESSING.ToString()).ToList();
                foreach (var cls in classFiltered)
                {
                    var schedules = new List<ScheduleMessage>();
                    int order = 0;
                    foreach (var schedule in cls.Schedules.OrderBy(sc => sc.Schedule))
                    {
                        schedules.Add(new ScheduleMessage
                        {
                            Slot = schedule.Slot!,
                            DayOfWeek = schedule.Schedule!,
                            Order = order++,
                        });
                    }
                    classes.Add(new ClassDefaultMessage
                    {
                        ClassId = cls.ClassId.ToString(),
                        ClassCode = cls.ClassName!,
                        CourseBeLong = cls.CourseName!,
                        StartDate = cls.StartDate.ToString("MM/dd/yyyy"),
                        LecturerBeLong = cls.Lecture!.FullName!,
                        LecturerPhone = cls.Lecture.Phone,
                        Schedules = schedules,
                    });
                }
                Classes = classes;
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Classes", Classes);
            }
        }
        public async Task<IActionResult> OnPostAsync(int inputField, string listId, string submitButton)
        {
            if (submitButton == "Refresh")
            {
                CurrentStudentQuizInforMessage = null;
                Classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
                return Page();
            }

            if (string.IsNullOrEmpty(listId))
            {
                ViewData["Message"] = "Lớp Chưa Được Chọn";
                CurrentStudentQuizInforMessage = null;
                Classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext.Session, "Classes");
                IsLoading = true;
                return Page();
            }

            ViewData["Message"] = "";

            var idParses = new List<string>();
            if (!string.IsNullOrEmpty(listId))
            {
                string pattern = @"\|([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\|";
                MatchCollection matches = Regex.Matches(listId, pattern);

                foreach (Match match in matches)
                {
                    idParses.Add(match.Groups[1].Value);
                }
            }

            var classes = SessionHelper.GetObjectFromJson<List<ClassDefaultMessage>>(HttpContext!.Session, "Classes");
            if (idParses.Any())
            {
                classes = idParses.Select(id => classes.Single(c => c.ClassId == id)).ToList();
            }

            Random random = new Random();
            var studentQuizInforMessages = new List<StudentQuizInforMessage>();
            var defaultToken = SessionHelper.GetObjectFromJson<string>(HttpContext.Session, "DeveloperToken");

            foreach (var cls in classes)
            {
                var result = await _apiHelper.FetchApiAsync<StudentAuthenAndExam>(ApiEndpointConstant.DeveloperEndpoint.GetStudentAuthenAndExam + $"?classId={cls.ClassId}", MethodEnum.GET, null);
                var students = result.Data.StudentAuthen;
                var exams = result.Data.Exams;
                var gradeOffline = new List<StudentExamGrade>();

                foreach (var stu in students)
                {
                    SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", stu.AccessToken!);
                    var quizMessages = new List<QuizMessage>();
                    foreach (var exam in exams)
                    {
                        var quizInfor = new QuizRequest
                        {
                            ClassId = Guid.Parse(cls.ClassId),
                            ExamId = exam.ExamId,
                        };
                        var numberInCorrectAnswer = 0;

                        if (exam.QuizType == "offline")
                        {
                            SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", defaultToken);
                            gradeOffline.Clear();

                            var score = random.Next(5, 11);
                            gradeOffline.Add(new StudentExamGrade
                            {
                                StudentId = stu.UserId!.Value,
                                Score = score,
                                Status = EvaluateData.GetQuizEvaluate(score, random),
                            });

                            var quizResult = await _apiHelper.FetchApiAsync<string>(
                            ApiEndpointConstant.QuizEndpoint.GradeQuizOffLine + $"?ClassId={quizInfor.ClassId}" + $"&ExamId={quizInfor.ExamId}" + $"&isCheckingTime={false}", MethodEnum.POST, gradeOffline);
                        }
                        else
                        {
                            await QuizOnlineProgress(inputField, random, cls, exam, numberInCorrectAnswer, quizInfor);
                            string status = EvaluateData.GetQuizEvaluate(random.Next(5, 11), random);

                            SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", defaultToken);
                            var evaluateQuiz = await _apiHelper.FetchApiAsync<string>(
                            ApiEndpointConstant.QuizEndpoint.EvaluateQuizOnLine + $"?studentId={stu.UserId}" + $"&examId={exam.ExamId}" + $"&status={status}", MethodEnum.POST, null);
                        }

                        SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", stu.AccessToken!);
                    }

                    var allCurrentStudentTestResult = await _apiHelper.FetchApiAsync<List<QuizResultExtraInforResponse>>(ApiEndpointConstant.QuizEndpoint.GetCurrentStudentQuizDone, MethodEnum.GET, null);
                    foreach (var test in allCurrentStudentTestResult.Data)
                    {
                        quizMessages.Add(new QuizMessage
                        {
                            QuizName = test.QuizName!,
                            NoAttempt = test.NoAttempt,
                            TotalMark = test.TotalMark,
                            CorrectMark = test.CorrectMark,
                            TotalScore = test.TotalScore,
                            ScoreEarned = test.ScoreEarned,
                            ExamStatus = test.ExamStatus,
                        });
                    }
                    studentQuizInforMessages.Add(new StudentQuizInforMessage
                    {
                        StudentName = stu.FullName!,
                        Quizzes = quizMessages,
                    });
                }
            }

            CurrentStudentQuizInforMessage = studentQuizInforMessages.First();
            ViewData["IndexPage"] = 0;
            SessionHelper.SetObjectAsJson(HttpContext.Session, "DataQuiz", studentQuizInforMessages);
            SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", defaultToken);

            IsLoading = true;

            return Page();
        }

        private async Task QuizOnlineProgress(int inputField, Random random, ClassDefaultMessage cls, ExamResForStudent exam, int numberInCorrectAnswer, QuizRequest quizInfor)
        {
            var quiz = await _apiHelper.FetchApiAsync<List<QuizResponse>>(
                                        ApiEndpointConstant.QuizEndpoint.GetQuizOffExamByExamId + $"?id={exam.ExamId}" + $"&classId={cls.ClassId}", MethodEnum.GET, null);

            if (exam.QuizType == "flashcard")
            {
                await QuizFlashCardProgress(inputField, random, numberInCorrectAnswer, quiz, quizInfor);
            }
            else
            {
                await QuizMultipleChoiceProgress(inputField, random, quiz, quizInfor);
            }

        }

        private async Task QuizMultipleChoiceProgress(int inputField, Random random, ResultHelper<List<QuizResponse>> quiz, QuizRequest quizInfor)
        {
            int numberInCorrectAnswer = 0;
            var workRequest = new List<MCStudentAnswer>();
            var inCorrectIndexStored = new List<int>();
            int order = 0;

            numberInCorrectAnswer = (inputField * quiz.Data.Count) / 100;
            for (int i = 0; i < numberInCorrectAnswer; i++)
            {
                int incorrectIndex;
                do
                {
                    incorrectIndex = random.Next(0, quiz.Data.Count);
                } while (inCorrectIndexStored.Contains(incorrectIndex));
                inCorrectIndexStored.Add(incorrectIndex);
            }

            foreach (var question in quiz.Data)
            {
                order++;
                var multipleChoiceAnswer = question.AnswersMutipleChoicesInfor;

                var answer = inCorrectIndexStored.Contains(order)
                    ? multipleChoiceAnswer!.FirstOrDefault(x => x.Score == 0)
                    : multipleChoiceAnswer!.FirstOrDefault(x => x.Score != 0);

                workRequest.Add(new MCStudentAnswer
                {
                    QuestionId = question.QuestionId,
                    AnswerId = answer!.AnswerId,
                });
            }


            var quizResult = await _apiHelper.FetchApiAsync<QuizResultResponse>(
                              ApiEndpointConstant.QuizEndpoint.GradeQuizMC + $"?ClassId={quizInfor.ClassId}" + $"&ExamId={quizInfor.ExamId}" + $"&doingTime={TimeSpan.FromMinutes(1)}" + $"&isCheckingTime={false}", MethodEnum.POST, workRequest);
        }

        private async Task QuizFlashCardProgress(int inputField, Random random, int numberInCorrectAnswer, ResultHelper<List<QuizResponse>> quiz, QuizRequest quizInfor)
        {
            var workRequest = new List<FCStudentQuestion>();
            var inCorrectIndexStored = new List<int>();

            foreach (var question in quiz.Data)
            {
                var flashCardAnswers = question.AnwserFlashCarsInfor;
                numberInCorrectAnswer = (inputField * flashCardAnswers!.Count / 2) / 100;

                for (int i = 0; i < numberInCorrectAnswer; i++)
                {
                    int incorrectIndex;
                    do
                    {
                        incorrectIndex = random.Next(0, flashCardAnswers!.Count / 2);
                    } while (inCorrectIndexStored.Contains(incorrectIndex));
                    inCorrectIndexStored.Add(incorrectIndex);
                }

                var answerStored = new List<Guid>();
                var answerRequest = new List<FCStudentAnswer>();
                for (int j = 0; j < flashCardAnswers!.Count / 2; j++)
                {
                    FCAnswerResponse firstCard;
                    do
                    {
                        firstCard = flashCardAnswers[random.Next(0, flashCardAnswers!.Count)];
                    } while (answerStored.Contains(firstCard.CardId));

                    answerStored.Add(firstCard.CardId);
                    var secondCard = inCorrectIndexStored.Contains(j)
                        ? flashCardAnswers.FirstOrDefault(fc => fc.NumberCoupleIdentify != firstCard.NumberCoupleIdentify)
                        : flashCardAnswers.FirstOrDefault(fc => fc.NumberCoupleIdentify == firstCard.NumberCoupleIdentify && !answerStored.Contains(fc.CardId));

                    if (!inCorrectIndexStored.Contains(j))
                    {
                        answerStored.Add(secondCard!.CardId);
                    }
                    else
                    {
                        var nonAnswerSecondCard = flashCardAnswers.FirstOrDefault(fc => fc.NumberCoupleIdentify == firstCard.NumberCoupleIdentify && !answerStored.Contains(fc.CardId));
                        answerStored.Add(nonAnswerSecondCard!.CardId);
                    }


                    answerRequest.Add(new FCStudentAnswer
                    {
                        FirstCardId = firstCard.CardId,
                        SecondCardId = secondCard!.CardId,
                    });
                };

                workRequest.Add(new FCStudentQuestion
                {
                    QuestionId = question.QuestionId,
                    Answers = answerRequest,
                });
            }

            var quizResult = await _apiHelper.FetchApiAsync<QuizResultResponse>(
                                   ApiEndpointConstant.QuizEndpoint.GradeQuizFC + $"?ClassId={quizInfor.ClassId}" + $"&ExamId={quizInfor.ExamId}" + $"&doingTime={TimeSpan.FromMinutes(1)}" + $"&isCheckingTime={false}", MethodEnum.POST, workRequest);
        }

        public IActionResult OnPostTableControl(string indexPage, string tableButtonSubmit)
        {
            var classes = SessionHelper.GetObjectFromJson<List<StudentQuizInforMessage>>(HttpContext.Session, "DataQuiz");
            int parseIndex = int.Parse(indexPage);
            int newIndex = tableButtonSubmit == "Next" ? parseIndex + 1 : parseIndex - 1;

            if (parseIndex == 0 && tableButtonSubmit == "Previous")
            {
                newIndex = parseIndex;
            }
            if (parseIndex == classes.Count - 1 && tableButtonSubmit == "Next")
            {
                newIndex = parseIndex;
            }

            CurrentStudentQuizInforMessage = classes[newIndex];
            ViewData["IndexPage"] = newIndex;

            return Page();
        }


    }
}
