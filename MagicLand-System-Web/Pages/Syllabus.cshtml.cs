using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response;
using MagicLand_System_Web_Dev.Pages.DataContants;
using MagicLand_System_Web_Dev.Pages.Enums;
using MagicLand_System_Web_Dev.Pages.Helper;
using MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MagicLand_System_Web_Dev.Pages
{
    public class SyllabusModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public SyllabusModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public List<SyllabusDefaultMessage> SyllabusMessages { get; set; } = new List<SyllabusDefaultMessage>();


        [BindProperty]
        public bool IsLoading { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IsLoading = false;
            var token = SessionHelper.GetObjectFromJson<string>(HttpContext.Session, "Token");
            var data = SessionHelper.GetObjectFromJson<List<SyllabusDefaultMessage>>(HttpContext.Session, "DataSyllabus");

            if (data != null && data.Count > 0)
            {
                SyllabusMessages = data;
            }

            if (token != null)
            {
                return Page();
            }

            var objectRequest = new LoginRequest
            {
                Phone = "+84971822093",
            };

            var result = await _apiHelper.FetchApiAsync<LoginResponse>(ApiEndpointConstant.AuthenticationEndpoint.Authentication, MethodEnum.POST, objectRequest);

            if (result.IsSuccess)
            {
                var user = result.Data;
                SessionHelper.SetObjectAsJson(HttpContext.Session, "Token", user!.AccessToken);
                SessionHelper.SetObjectAsJson(HttpContext.Session, "DeveloperToken", user!.AccessToken);
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int inputField)
        {
            if (inputField == 0 || inputField < 0 || inputField >= 100)
            {
                ViewData["Message"] = "Số Lượng không Hợp Lệ";
                return Page();
            }
            Random random = new Random();

            var subjects = SyllabusData.Subjects;



            for (int i = 0; i < inputField; i++)
            {
                int index = random.Next(0, 1000), indexSubject = random.Next(0, subjects.Count);

                var subject = subjects[indexSubject];

                var generateResult = GenerateSyllabus();
                var syllabusNameItems = SyllabusData.GetSyllabusName(subject.Item2);

                var objectRequest = new OverallSyllabusRequest
                {
                    Description = SyllabusData.GetSyllabusDescription(subject.Item2),
                    MinAvgMarkToPass = random.Next(4, 5),
                    SyllabusName = syllabusNameItems.Item1 + "-" + index,
                    ScoringScale = 10,
                    StudentTasks = "Hoàn thành các khóa học, thực hiện đầy đủ các bài tập và làm bài kiểm tra.",
                    SubjectCode = syllabusNameItems.Item2 + "-" + index,
                    SyllabusLink = "https://firebasestorage.googleapis.com/v0/b/magic-2e5fc.appspot.com/o/syllabuses%2FTo%C3%A1n%20t%C6%B0%20duy%20cho%20b%C3%A9%2F28%2F2%2F...2b1dd733",
                    TimePerSession = index % 2 == 0 ? 60 : 90,
                    NumOfSessions = 20,
                    EffectiveDate = DateTime.Now.ToString(),
                    Type = subject.Item1,
                    MaterialRequests = GenerateMaterial(),
                    SyllabusRequests = generateResult.Item1,
                    ExamSyllabusRequests = generateResult.Item2,
                    QuestionPackageRequests = generateResult.Item3,
                };

                var result = await _apiHelper.FetchApiAsync<string>(ApiEndpointConstant.SyllabusEndpoint.AddSyllabus, MethodEnum.POST, objectRequest);

                IsLoading = true;

                SyllabusMessages.Add(new SyllabusDefaultMessage
                {
                    SyllabusName = objectRequest.SyllabusName,
                    Status = result.StatusCode,
                    Subject = objectRequest.Type,
                    SyllabusCode = objectRequest.SubjectCode,
                    Note = result.Message,
                });

                SessionHelper.SetObjectAsJson(HttpContext.Session, "DataSyllabus", SyllabusMessages);

            }

            return Page();
        }

        private List<MaterialRequest> GenerateMaterial()
        {
            return new List<MaterialRequest>
            {
                 new MaterialRequest
                        {
                            URL = "https://firebasestorage.googleapis.com/v0/b/magic-2e5fc.appspot.com/o/syllabuses%2FTo%C3%A1n%20t%C6%B0%20duy%20cho%20b%C3%A9%2F28%2F2%2F...293b346b",
                            FileName = "file.name",
                        },
            };
        }

        private (List<SyllabusRequest>, List<ExamSyllabusRequest>, List<QuestionPackageRequest>) GenerateSyllabus()
        {
            Random random = new Random();

            var syllabusRequest = new List<SyllabusRequest>();
            var exams = new List<ExamSyllabusRequest>();
            var questionPackages = new List<QuestionPackageRequest>();

            int orderSession = 0;

            exams.Add(new ExamSyllabusRequest
            {
                Type = "Participation",
                ContentName = "Điểm danh",
                Weight = 10,
                CompleteionCriteria = 0,
                Part = 1,
                QuestionType = null,
                Duration = null,
                Method = "online",
            });

            for (int i = 0; i < 10; i++)
            {
                var sessions = new List<SessionRequest>();

                for (int j = 0; j < 2; j++)
                {
                    orderSession++;
                    var type = GenerateExam(orderSession, random, exams);
                    GenerateQuestionPackage(random, orderSession, type, questionPackages);

                    sessions.Add(new SessionRequest
                    {
                        Order = orderSession,
                        SessionContentRequests = new List<SessionContentRequest>
                         {
                             new SessionContentRequest
                             {
                                 Content = orderSession == 6 || orderSession == 15 ? "Luyện tập"
                                 : orderSession == 11 ? "Kiểm tra"
                                 :  orderSession == 20 ? "Kiểm tra cuối khóa"
                                 : "Đây Là Chủ Đề Cho Buổi Học Thứ Tự " + orderSession,
                                 SessionContentDetails = new List<string>
                                 {
                                     "Đây Là Mô Tả Chi Tiết Đầu Tiên Cho Buổi Học Thứ Tự " + orderSession,
                                      "Đây Là Mô Tả Chi Tiết Thứ Hai Cho Buổi Học Thứ Tự " + orderSession,
                                 }
                             }
                         }
                    });
                }

                syllabusRequest.Add(new SyllabusRequest
                {
                    Index = i + 1,
                    TopicName = "Đây Là Tên Của Chủ Đề Có Thứ Tự " + (i + 1),
                    SessionRequests = sessions,
                });
            }

            return (syllabusRequest, exams, questionPackages);
        }

        private void GenerateQuestionPackage(Random random, int orderSession, string quizType, List<QuestionPackageRequest> questionPackages)
        {
            string contentName = string.Empty;
            var index = random.Next(0, SyllabusData.QuizType.Count);
            switch (orderSession)
            {
                case 6:
                    contentName = "Luyện tập";
                    break;
                case 15:
                    contentName = "Luyện tập";
                    break;
                case 17:
                    contentName = "Ôn tập";
                    quizType = SyllabusData.QuizType[index].Item2;
                    break;
                case 19:
                    contentName = "Ôn tập";
                    quizType = SyllabusData.QuizType[index].Item2;
                    break;
                case 20:
                    contentName = "Kiểm tra cuối khóa";
                    break;
            }

            if (string.IsNullOrEmpty(contentName) || string.IsNullOrEmpty(quizType))
            {
                return;
            }

            var questions = new List<QuestionRequest>();

            if (quizType == "MUL")
            {
                var numberQuestion = random.Next(10, 16);

                for (int i = 0; i < numberQuestion; i++)
                {
                    var mutipleChoices = new List<MutipleChoiceAnswerRequest>();
                    GenerateMutilpleChoice(random, i, mutipleChoices);

                    questions.Add(new QuestionRequest
                    {
                        Description = "Đây Là Mô Tả Câu Hỏi Trắc Nghiệm Có Thứ Tự " + (i + 1),
                        Img = "img.png",
                        MutipleChoiceAnswerRequests = mutipleChoices,
                    });
                }
            }

            if (quizType == "FLA")
            {
                var numberQuestion = random.Next(5, 8);

                for (int j = 0; j < numberQuestion; j++)
                {
                    var flashCards = new List<FlashCardRequest>();
                    GenerateFlashCard(random, numberQuestion, flashCards);

                    questions.Add(new QuestionRequest
                    {
                        Description = "Đây Là Mô Tả Câu Hỏi Nối Thẻ Có Thứ Tự " + (j + 1),
                        Img = "img.png",
                        FlashCardRequests = flashCards,
                    });
                }
            }


            questionPackages.Add(new QuestionPackageRequest
            {
                ContentName = contentName,
                NoOfSession = orderSession,
                Type = quizType == "MUL" ? "multiple-choice" : "flashcard",
                Title = "Đây Là Tiêu Đề Cho Gói Câu Hỏi Thuộc Buổi Học " + orderSession,
                Score = 10,
                QuestionRequests = questions,
            });
        }

        private void GenerateFlashCard(Random random, int orderQuestion, List<FlashCardRequest> flashCard)
        {
            var numberPairCard = random.Next(5, 9);
            for (int k = 0; k < numberPairCard; k++)
            {
                flashCard.Add(new FlashCardRequest
                {
                    RightSideDescription = "Đây Là Mô Tả Chi Tiết Cho Thẻ Đầu Tiên Có Thứ Tự " + (k + 1) + " Cho Câu Hỏi Có Thứ Tự " + (orderQuestion + 1),
                    LeftSideDescription = "Đây Là Mô Tả Chi Tiết Cho Thẻ Thứ Hai Có Thứ Tự " + (k + 1) + " Cho Câu Hỏi Có Thứ Tự " + (orderQuestion + 1),
                    Score = 1,
                });

            }
        }

        private void GenerateMutilpleChoice(Random random, int orderQuestion, List<MutipleChoiceAnswerRequest> multipleChoiceAnswers)
        {
            int answerSucces = random.Next(0, 3);
            for (int k = 0; k < 4; k++)
            {
                multipleChoiceAnswers.Add(new MutipleChoiceAnswerRequest
                {
                    Description = k == answerSucces
                    ? "Đây Là Mô Tả Câu Trả Đúng Cho Câu Hỏi Có Thứ Tự " + (orderQuestion + 1)
                    : "Đây Là Mô Tả Câu Trả Lời Sai Cho Câu Hỏi Có Thứ Tự " + (orderQuestion + 1),
                    Img = "https://drive.google.com/thumbnail?id=1P7IvweybpPEqSSmW1146O1Hn_YJAWZ6Q",
                    Score = k == answerSucces ? 1 : 0,
                });

            }
        }

        private string GenerateExam(int orderSession, Random random, List<ExamSyllabusRequest> exams)
        {
            var index = random.Next(0, SyllabusData.QuizType.Count);
            switch (orderSession)
            {
                case 6:
                    exams.Add(new ExamSyllabusRequest
                    {
                        Type = "Practice",
                        ContentName = "Luyện Tập",
                        Weight = 20,
                        CompleteionCriteria = 0,
                        QuestionType = SyllabusData.QuizType[index].Item1,
                        Part = 2,
                        Method = "online",
                        Duration = "600",
                    });
                    break;
                case 15:
                    break;
                case 11:
                    exams.Add(new ExamSyllabusRequest
                    {
                        Type = "Progress Test",
                        ContentName = "Kiểm tra",
                        Weight = 30,
                        CompleteionCriteria = 0,
                        QuestionType = "Giáo Viên Tự Chọn",
                        Part = 1,
                        Method = "offline",
                        Duration = "300",
                    });
                    return string.Empty;
                case 20:
                    exams.Add(new ExamSyllabusRequest
                    {
                        Type = "Final Exam",
                        ContentName = "Kiểm tra cuối khóa",
                        Weight = 40,
                        CompleteionCriteria = 0,
                        QuestionType = SyllabusData.QuizType[index].Item1,
                        Part = 1,
                        Method = "online",
                        Duration = "600",
                    });
                    break;
            }

            return SyllabusData.QuizType[index].Item2;
        }
    }
}
