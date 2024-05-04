using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response;
using MagicLand_System_Web.Pages.DataContants;
using MagicLand_System_Web.Pages.Enums;
using MagicLand_System_Web.Pages.Helper;
using MagicLand_System_Web.Pages.Message;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MagicLand_System_Web.Pages
{
    public class SyllabusModel : PageModel
    {
        private readonly ApiHelper _apiHelper;

        public SyllabusModel(ApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        [BindProperty]
        public List<SyllabusMessage> SyllabusMessages { get; set; } = new List<SyllabusMessage>();

        [BindProperty]
        public bool IsLoading { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IsLoading = false;
            var token = SessionHelper.GetObjectFromJson<string>(HttpContext.Session, "Token");
            var data = SessionHelper.GetObjectFromJson<List<SyllabusMessage>>(HttpContext.Session, "DataSyllabus");

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

            //var subjects = new List<(string, string)>
            //{
            //    ("Nhạc", "NA"),("Ngôn Ngữ", "NN"), ("Toán", "TTD"), ("Nhảy", "NB"), ("Hát", "H"), ("Vật Lý", "VL"), ("Lập Trình", "LT"), ("Hội Họa", "HH")
            //};
            var subjects = SyllabusData.Subjects;

            Random random = new Random();

            for (int i = 0; i < inputField; i++)
            {
                int index = random.Next(0, 1000), indexSubject = random.Next(0, subjects.Count);

                var subject = subjects[indexSubject];

                var objectRequest = new OverallSyllabusRequest
                {
                    Description = SyllabusData.GetSyllabusInfor(subject.Item2, TypeDataEnum.Description),
                    MinAvgMarkToPass = random.Next(4, 5),
                    SyllabusName = SyllabusData.GetSyllabusInfor(subject.Item2, TypeDataEnum.SyllabusName) + "-" + index,
                    ScoringScale = 10,
                    StudentTasks = "Hoàn thành các khóa học, thực hiện đầy đủ các bài tập và làm bài kiểm tra.",
                    SubjectCode = SyllabusData.GetSyllabusInfor(subject.Item2, TypeDataEnum.SyllabusCode) + "-" + index,
                    SyllabusLink = "https://firebasestorage.googleapis.com/v0/b/magic-2e5fc.appspot.com/o/syllabuses%2FTo%C3%A1n%20t%C6%B0%20duy%20cho%20b%C3%A9%2F28%2F2%2F...2b1dd733",
                    TimePerSession = index % 2 == 0 ? 60 : 90,
                    NumOfSessions = 20,
                    EffectiveDate = DateTime.Now.ToString(),
                    Type = subject.Item1,
                    MaterialRequests = GenerateMaterial(),
                    SyllabusRequests = GenerateSyllabus(),
                    ExamSyllabusRequests = GenerateExams(),
                    QuestionPackageRequests = GenerateQuestionPackage(),
                };


                //string token = SessionHelper.GetObjectFromJson<string>(_httpContextAccessor.HttpContext!.Session, "Token");
                //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                //var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                //var insertResponse = await _httpClient.PostAsync(baseUrl + "/Syllabus/insertSyllabus", jsonContent);

                //int statusCode = (int)insertResponse.StatusCode;
                //string responseMessage = await insertResponse.Content.ReadAsStringAsync();

                //IsLoading = true;

                //SyllabusMessages.Add(new SyllabusMessage
                //{
                //    SyllabusName = requestData.SyllabusName,
                //    Status = statusCode.ToString(),
                //    Subject = requestData.Type,
                //    SyllabusCode = requestData.SubjectCode,
                //    Note = responseMessage,
                //});
                //SessionHelper.SetObjectAsJson(HttpContext.Session, "DataSyllabus", SyllabusMessages);

                var result = await _apiHelper.FetchApiAsync<string>(ApiEndpointConstant.SyllabusEndpoint.AddSyllabus, MethodEnum.POST, objectRequest);

                IsLoading = true;

                SyllabusMessages.Add(new SyllabusMessage
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

        private List<SyllabusRequest> GenerateSyllabus()
        {
            var syllabusRequest = new List<SyllabusRequest>();
            int order = 0;
            for (int i = 0; i < 10; i++)
            {
                var sessions = new List<SessionRequest>();

                for (int j = 0; j < 2; j++)
                {
                    order++;
                    sessions.Add(new SessionRequest
                    {
                        Order = order,
                        SessionContentRequests = new List<SessionContentRequest>
                         {
                             new SessionContentRequest
                             {
                                 Content = "Đây Là Chủ Đề Cho Buổi Học Thứ Tự " + order,
                                 SessionContentDetails = new List<string>
                                 {
                                     "Đây Là Mô Tả Chi Tiết Đầu Tiên Cho Buổi Học Thứ Tự " + order,
                                      "Đây Là Mô Tả Chi Tiết Thứ Hai Cho Buổi Học Thứ Tự " + order,
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

            return syllabusRequest;
        }

        private List<QuestionPackageRequest> GenerateQuestionPackage()
        {

            Random random = new Random();

            var questionPackage = new List<QuestionPackageRequest>();
            for (int i = 0; i < 3; i++)
            {
                var question = new List<QuestionRequest>();

                for (int j = 0; j < 3; j++)
                {
                    var mutiple = new List<MutipleChoiceAnswerRequest>();
                    var flashCard = new List<FlashCardRequest>();

                    if (j == 0)
                    {
                        GenerateMutilpleChoice(random, mutiple);

                    }
                    else
                    {
                        GenerateFlashCard(flashCard);
                    }

                    question.Add(new QuestionRequest
                    {
                        Description = "Đây Là Mô Tả Câu Hỏi Có Thứ Tự " + (j + 1),
                        Img = "img.png",
                        MutipleChoiceAnswerRequests = mutiple,
                        FlashCardRequests = flashCard,
                    });
                }

                questionPackage.Add(new QuestionPackageRequest
                {
                    ContentName = i == 0 ? "Luyện Tập" : i == 1 ? "Luyện Tập" : "Kiểm Tra Cuối Khóa",
                    NoOfSession = i == 0 ? 6 : i == 1 ? 15 : 20,
                    Type = i == 0 ? "multiple-choice" : i == 1 ? "flashcard" : "flashcard",
                    Title = "Đây Là Tiêu Đề Cho Gói Câu Hỏi Thứ Tự " + (i + 1),
                    Score = 10,
                    QuestionRequests = question,
                });
            }

            return questionPackage;
        }

        private void GenerateFlashCard(List<FlashCardRequest> flashCard)
        {
            for (int k = 0; k < 4; k++)
            {
                flashCard.Add(new FlashCardRequest
                {
                    RightSideDescription = "Đây Là Chi Tiết Cho Thẻ Đầu Tiên Thuộc Thứ Tự " + (k + 1),
                    LeftSideDescription = "Đây Là Chi Tiết Cho Thẻ Thứ Hai Thuộc Thứ Tự " + (k + 1),
                    Score = 1,
                });

            }
        }

        private void GenerateMutilpleChoice(Random random, List<MutipleChoiceAnswerRequest> mutiple)
        {
            int answerSucces = random.Next(0, 3);
            for (int k = 0; k < 4; k++)
            {
                mutiple.Add(new MutipleChoiceAnswerRequest
                {
                    Description = "Đây Là Mô Tả Câu Trả Lời Có Thứ Tự" + (k + 1),
                    Img = "https://drive.google.com/thumbnail?id=1P7IvweybpPEqSSmW1146O1Hn_YJAWZ6Q",
                    Score = k == answerSucces ? 1 : 0,
                });

            }
        }

        private List<ExamSyllabusRequest> GenerateExams()
        {
            var exams = new List<ExamSyllabusRequest>();
            for (int i = 0; i < 3; i++)
            {
                exams.Add(new ExamSyllabusRequest
                {
                    Type = i == 0 ? "Practice" : i == 1 ? "Test" : "FinalExam",
                    ContentName = i == 0 ? "Luyện Tập" : i == 1 ? "Kiểm Tra" : "Kiểm Tra Cuối Khóa",
                    Weight = 30,
                    CompleteionCriteria = 0,
                    QuestionType = i == 0 ? "Trắc nghiệm" : i == 1 ? "Tùy Chọn" : "Ghép thẻ",
                    Part = i == 0 ? 2 : 1,
                    Method = i == 0 ? "Online" : i == 1 ? "Offline" : "Online",
                    Duration = i == 0 ? "30" : i == 1 ? "Tại Nhà" : "40",
                });
            }
            exams.Add(new ExamSyllabusRequest
            {
                Type = "Participation",
                ContentName = "Điểm danh",
                Weight = 10,
                CompleteionCriteria = 0,
                Part = 1,
                Method = "Online",
            });

            return exams;
        }
    }
}
