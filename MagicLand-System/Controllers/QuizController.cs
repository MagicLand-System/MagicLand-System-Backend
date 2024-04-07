using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Request.Quizzes;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
using MagicLand_System.PayLoad.Response.Quizzes.Result;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Final;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Student;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class QuizController : BaseController<QuizController>
    {
        private readonly ISyllabusService _syllabusService;
        private readonly IQuizService _quizService;

        public QuizController(ILogger<QuizController> logger, ISyllabusService syllabusService, IQuizService quizService) : base(logger)
        {
            _syllabusService = syllabusService;
            _quizService = quizService;
        }

        #region document API Get Exams With Quiz
        /// <summary>
        ///  Truy Suất Toàn Bộ Bài Kiểm Tra Kèm Bộ Đề Và Câu Hỏi (Quiz) Của Các Khóa Học Đã Có Giáo Trình
        /// </summary>
        /// <response code="200">Trả Về Các Bài Kiểm Tra Kèm Bộ Đề Của Các Khóa Học</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.EndPointBase)]
        [ProducesResponseType(typeof(ExamWithQuizResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuizzes()
        {
            var responses = await _syllabusService.LoadQuizzesAsync();

            return Ok(responses);
        }

        #region document API Get Exams With Quiz By Course Id
        /// <summary>
        ///  Truy Suất Toàn Bộ Bài Kiểm Tra Kèm Bộ Đề Và Câu Hỏi (Quiz) Của Một Khóa Học Cụ Thể Dựa Vào Id Của Khóa
        /// </summary>
        /// <param name="id">Id Của Khóa Học</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "id":"3c1849af-400c-43ca-979e-58c71ce9301d"  
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Các Bài Kiểm Tra Kèm Bộ Đề Của Khóa Học</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetQuizOverallByCourseId)]
        [ProducesResponseType(typeof(ExamWithQuizResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuizByCourseId([FromQuery] Guid id)
        {
            var responses = await _syllabusService.LoadQuizzesByCourseIdAsync(id);

            return Ok(responses);
        }
        #region document API Get Question Package Flas Card
        /// <summary>
        ///  Truy Suất Toàn Bộ Gói Câu Hỏi Và Câu Trả Lời Của Một Bài Kiểm Tra (Chỉ Áp Dụng Dạng Nối Thẻ)
        /// </summary>
        /// <param name="examId">Id Của Bài Kiểm Tra Nối Thẻ</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "examId":"3c1849af-400c-43ca-979e-58c71ce9301d" ,
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Các Câu Hỏi Và Cặp Thẻ Trả Lời</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        //[HttpGet(ApiEndpointConstant.QuizEndPoint.GetFCQuestionPackage)]
        //[ProducesResponseType(typeof(FCQuizResponse), StatusCodes.Status200OK)]
        //[ProducesErrorResponseType(typeof(Exception))]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetFCQuestionPackage([FromQuery] Guid examId)
        //{
        //    var responses = await _quizService.GetFCQuestionPackageAsync(examId);

        //    return Ok(responses);
        //}

        #region document API Get Exams Of Class By Class Id
        /// <summary>
        ///  Truy Suất Các Bài Kiểm Tra Của Một Lớp Học, Nếu Có Id Của Học Sinh Sẽ Kiểm Tra Và Truất Thông Tin Bài Tập Của Học Sinh Đó
        /// </summary>
        /// <param name="id">Id Của Lớp Học</param>
        /// <param name="studentId">Id Của Học Sinh (Option)</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "id":"3c1849af-400c-43ca-979e-58c71ce9301d" ,
        ///    "studentId": "EC4C3593-7C58-423C-A87A-CEF2A391C57a"
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Các Bài Kiểm Tra Của Lớp Học</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetExamOffClassByClassId)]
        [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> GetExamOfClassByClassId([FromQuery] Guid id, [FromQuery] Guid? studentId)
        {
            var responses = await _syllabusService.LoadExamOfClassByClassIdAsync(id, studentId);

            return Ok(responses);
        }

        #region document API Get Fully Exam Infor
        /// <summary>
        ///  Truy Suất Thông Tin Bài Kiểm Tra Của Học Sinh
        /// </summary>
        /// <param name="examId">Id Của Bài Kiểm Tra</param>
        /// <param name="studentId">Id Của Học Sinh</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "examId":"3c1849af-400c-43ca-979e-58c71ce9301d" ,
        ///    "studentId": "EC4C3593-7C58-423C-A87A-CEF2A391C57a"
        ///}
        /// </remarks>
        /// <response code="200">Trả Thông Tin Bài Kiểm Tra</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        //[HttpGet(ApiEndpointConstant.QuizEndPoint.)]
        //[ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
        //[ProducesErrorResponseType(typeof(Exception))]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetExamOfClassByClassId([FromQuery] Guid examId, [FromQuery] Guid studentId)
        //{
        //    var responses = await _quizService.(examId, studentId);

        //    return Ok(responses);
        //}

        #region document API Get Exams Of Current Student In Time
        /// <summary>
        ///  Truy Suất Các Bài Kiểm Tra Của Bé Trong Khoảng Thời Gian Trước Và Sau Ngày Hiện Tại
        /// </summary>
        /// <param name="numberOfDay">Số Ngày Trước Và Sau Ngày Hiện Tại Cần Truy Suất</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "numberOfDay": 3
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Các Bài Kiểm Tra</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetExamOffCurrentStudentByTime)]
        [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> GetExamsOfCurrentStudentByTime([FromQuery] int numberOfDay)
        {

            if (numberOfDay < 0 || numberOfDay > 30)
            {
                return BadRequest("Số Ngày Không Hợp Lệ");
            }

            var responses = await _syllabusService.LoadExamOfCurrentStudentAsync(numberOfDay);

            return Ok(responses);
        }

        #region document API Get Quizzes By Class Id
        /// <summary>
        ///  Truy Suất Các Câu Hỏi (Quiz) Trong Bộ Đề Của Một Bài Kiểm Tra Dựa Vào Id Của Bài Kiểm Tra, *Các Câu Hỏi Sẽ Được Truy Suất Ngẫu Nhiên Và Thỏa Mãn Số Điểm Của Bài Kiểm Tra*
        /// </summary>
        /// <param name="id">Id Của Bài Kiểm Tra</param>
        /// <param name="examPart">Phần Đề Của Bài Kiểm Tra (Dạng Kiểm Tra)</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "id":"3c1849af-400c-43ca-979e-58c71ce9301d" ,
        ///    "examPart": 1
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Quiz Của Bài Kiểm Tra</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetQuizOffExamByExamId)]
        [ProducesResponseType(typeof(QuizResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> GetQuizOfExamByExamId([FromQuery] Guid id, [FromQuery] int? examPart)
        {
            var responses = await _syllabusService.LoadQuizOfExamByExamIdAsync(id, examPart);
            if (responses == default)
            {
                return Ok("Bài Kiểm Tra Này Do Giáo Viên Tự Chọn Câu Hỏi Và Đề Tài");
            }

            return Ok(responses);
        }

        #region document API Grade Quiz OffLine
        /// <summary>
        ///  Lưu/Cập Nhập Điểm Và Đánh Giá Bài Kiểm Tra Làm Tại Nhà Của Các Học Sinh
        /// </summary>
        /// <param name="quizInfor">Chứa Thông Tin Của Bài Kiểm Tra Làm Tại Nhà</param>
        /// <param name="studentExamGrades">Chứa Id Của Các Học Sinh Và Điểm Của Giáo Viên</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "classId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///    "examId":"5229E1A5-79F9-48A5-B8ED-0A53F963CB29",
        ///    [
        ///      {
        ///        "studentId": "735616C5-B24A-4C16-A30A-A27A511CD6FA",
        ///        "score" : 10,
        ///        "status": "Làm Tốt Lắm"
        ///      },
        ///      ]
        /// </remarks>
        /// <response code="200">Trả Về Thông Báo</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.QuizEndPoint.GradeQuizOffLine)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> GradeQuizOffLine([FromQuery] QuizRequest quizInfor, [FromBody] List<StudentExamGrade> studentExamGrades)
        {
            var examOffLineStudentWork = new ExamOffLineRequest
            {
                ClassId = quizInfor.ClassId,
                ExamId = quizInfor.ExamId,
                StudentQuizGardes = studentExamGrades,
            };
            var response = await _quizService.GradeExamOffLineAsync(examOffLineStudentWork);

            return Ok(response);
        }

        #region document API Evaluate Quiz Online
        /// <summary>
        /// Lưu/Cập Nhập Đánh Giá Các Bài Kiểm Tra Đã Làm Trên Hệ Thống
        /// </summary>
        /// <param name="studentId">Id Của Học Sinh</param>
        /// <param name="examId">Id Của Bài Kiểm Tra</param>
        /// <param name="status">Trạng Thái Bài Kiểm Tra</param>
        /// <param name="noAttempt">Thứ Tự Lần Làm Kiểm Tra Của Học Sinh, Mặc Định [Lần Làm Gần Nhất] (Option) </param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "studentId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///    "examId":"5229E1A5-79F9-48A5-B8ED-0A53F963CB29",
        ///    "status" "Cần Cố Gắng",
        ///    "noAttempt": 1,
        /// </remarks>
        /// <response code="200">Trả Về Thông Báo</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.QuizEndPoint.EvaluateQuizOnLine)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> EvaluateQuizOnLine([FromQuery] Guid studentId, [FromQuery] Guid examId, [FromQuery] string status, [FromQuery] int? noAttempt)
        {
            var response = await _quizService.EvaluateExamOnLineAsync(studentId, examId, status, noAttempt);

            return Ok(response);
        }

        #region document API Get Grade Quiz Multiple Choice
        /// <summary>
        ///  Chấm Và Lưu Điểm Bài Kiểm Tra [Dạng Trắc Nghiệm] Của Học Sinh Hiện Tại
        /// </summary>
        /// <param name="quizInfor">Chứa Thông Tin Bài Kiểm Tra</param>
        /// <param name="studentWorkResults">Chứa Câu Hỏi Và Câu Trả Lời Của Học Sinh</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "classId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///    "examId":"5229E1A5-79F9-48A5-B8ED-0A53F963CB29",
        ///    [
        ///      {
        ///        "questionId": "735616C5-B24A-4C16-A30A-A27A511CD6FA",
        ///        "answerId" : "417997AC-AFD7-4363-BFE5-6CDD46D4712B"
        ///      },
        ///      ]
        /// </remarks>
        /// <response code="200">Trả Về Kết Quả Của Bài Kiểm Tra</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.QuizEndPoint.GradeQuizMC)]
        [ProducesResponseType(typeof(QuizResultResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> GradeQuizMC([FromQuery] QuizRequest quizInfor, [FromBody] List<MCStudentAnswer> studentWorkResults)
        {
            var duplicateQuestions = studentWorkResults
                .GroupBy(x => x.QuestionId)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .Distinct();

            var duplicateAnswers = studentWorkResults
               .GroupBy(x => x.AnswerId)
               .Where(g => g.Count() > 1)
               .SelectMany(g => g)
               .Distinct();

            if (duplicateQuestions != null && duplicateQuestions.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Có Nhiều Hơn Các Id Của Câu Hỏi Bị Trùng Lặp [{string.Join(", ", duplicateQuestions.Select(dq => dq.QuestionId))}]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            if (duplicateAnswers != null && duplicateAnswers.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Có Nhiều Hơn Các Id Của Câu Trả Lời Bị Trùng Lặp [{string.Join(", ", duplicateAnswers.Select(dq => dq.AnswerId))}]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var quizMcStudentWork = new QuizMCRequest
            {
                ClassId = quizInfor.ClassId,
                ExamId = quizInfor.ExamId,
                StudentQuestionResults = studentWorkResults,
            };
            var responses = await _quizService.GradeQuizMCAsync(quizMcStudentWork);

            return Ok(responses);
        }

        #region document API Get Grade Quiz Flash Card
        /// <summary>
        ///  Lưu Điểm Bài Kiểm Tra [Dạng Nối Thẻ] Của Học Sinh Hiện Tại
        /// </summary>
        /// <param name="quizInfor">Chứa Thông Tin Bài Kiểm Tra</param>
        /// <param name="studentWorkResults">Chứa Câu Hỏi Và Câu Trả Lời Của Học Sinh</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "classId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///    "examId":"5229E1A5-79F9-48A5-B8ED-0A53F963CB29",
        ///    [{
        ///      "questionId": "4729E1A5-79F9-48A5-B8ED-0A53F963Cc00",
        ///      {
        ///      "firstCardId": "71E936AB-8A76-442F-8795-975A154191A9",
        ///      "secondCardId": "55d89462-27c2-443f-2fd4-08dc3d07a27a"
        ///      },
        ///    }]
        /// </remarks>
        /// <response code="200">Trả Về Kết Quả Của Bài Kiểm Tra</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.QuizEndPoint.GradeQuizFC)]
        [ProducesResponseType(typeof(QuizResultResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> GradeQuizFC([FromQuery] QuizRequest quizInfor, [FromBody] List<FCStudentQuestion> studentWorkResults)
        {

            var studentAnswers = studentWorkResults.SelectMany(sr => sr.Answers).ToList();

            var duplicateCards = new List<Guid>();

            var duplicateQuestions = studentWorkResults
               .GroupBy(x => x.QuestionId)
               .Where(g => g.Count() > 1)
               .SelectMany(g => g)
               .Distinct();

            duplicateCards.AddRange(studentAnswers
           .GroupBy(x => x.FirstCardId)
           .Where(g => g.Count() > 1)
           .SelectMany(g => g)
           .Distinct().Select(x => x.FirstCardId));

            duplicateCards.AddRange(studentAnswers
            .GroupBy(x => x.SecondCardId)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .Distinct().Select(x => x.SecondCardId));

            if (duplicateQuestions != null && duplicateQuestions.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Có Nhiều Hơn Các Id Của Câu Hỏi Bị Trùng Lặp [{string.Join(", ", duplicateQuestions.Select(dq => dq.QuestionId))}]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            if (duplicateCards != null && duplicateCards.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Có Nhiều Hơn Các Id Của Thẻ Bị Trùng Lặp [{string.Join(", ", duplicateCards)}]",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var quizFCStudentWork = new QuizFCRequest
            {
                ClassId = quizInfor.ClassId,
                ExamId = quizInfor.ExamId,
                StudentQuestionResults = studentWorkResults,
            };
            //var responses = await _quizService.GradeQuizFCAsync(classId, examId, scoreEarned);
            var responses = await _quizService.GradeQuizFCAsync(quizFCStudentWork);

            return Ok(responses);
        }

        #region document API Get Test Result
        /// <summary>
        ///  Truy Suất Kết Quả Các Bài Kiểm Tra Đã Làm Của Học Sinh Hiện Tại
        /// </summary>
        /// <response code="200">Trả Về Chi Tiết Các Bài Kiểm Tra</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetCurrentStudentQuizDone)]
        [ProducesResponseType(typeof(QuizResultExtraInforResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> GetCurrentStudentQuizDone()
        {
            var responses = await _quizService.GetCurrentStudentQuizDoneAsync();

            return Ok(responses);
        }

        #region document API Get Test Student Work
        /// <summary>
        ///  Truy Suất Bài Làm Kiểm Tra Của Học Sinh Hiện Tại
        /// </summary>
        /// <param name="examId">Id Của Bài Kiểm Tra</param>
        /// <param name="noAttempt">Thứ Tự Lần Làm Bài, Mặc Định [Lần Làm Gần Nhất] (Option)</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "examId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///    "noAttempt":"1",
        /// </remarks>
        /// <response code="200">Trả Về Chi Tiết Bài Làm Của Học</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetCurrentStudentQuizWork)]
        [ProducesResponseType(typeof(StudentWorkResult), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> GetCurrentStudentQuizWork([FromQuery] Guid examId, [FromQuery] int? noAttempt)
        {
            var responses = await _quizService.GetCurrentStudentQuizDoneWorkAsync(examId, noAttempt);

            return Ok(responses);
        }

        #region document API Get Final Result
        /// <summary>
        ///  Truy Suất Kết Quả Tổng Kết Của Các Học Sinh
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Bảng Điểm Tổng Kết</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetFinalResult)]
        [ProducesResponseType(typeof(FinalResultResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize]
        public async Task<IActionResult> GetFinalResult([FromQuery] List<Guid> studentIdList)
        {
            if (studentIdList == null)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Yêu Cầu Không Hợp Lệ",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }

            var invalidIds = studentIdList.Where(id => id == default).ToList();
            if (invalidIds.Any())
            {
                return BadRequest(new ErrorResponse
                {
                    Error = $"Id [{string.Join(", ", invalidIds)}] Học Sinh Không Hợp Lệ",
                    StatusCode = StatusCodes.Status400BadRequest,
                    TimeStamp = DateTime.Now,
                });
            }
            var responses = await _quizService.GetFinalResultAsync(studentIdList);

            return Ok(responses);
        }

        #region document API Setting Exam
        /// <summary>
        ///  Cho Phép Giáo Viên Thiết Lập Hoặc Cập Nhập Thông Tin Thời Gian Bài Kiểm Tra
        /// </summary>
        /// <param name="quizInfor">Thông Tin Bài Kiểm Tra</param>
        /// <param name="quizStartTime">Thời Gian Bắt Đầu</param>
        /// <param name="quizEndTime">Thời Gian Kết Thúc</param>
        /// <param name="attemptAllowed">Số Lần Thử</param>
        /// <param name="duration">Thời Gian Làm Bài (Giây)</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "classId":"3c1849af-400c-43ca-979e-58c71ce9301d",
        ///    "examId":"5229E1A5-79F9-48A5-B8ED-0A53F963CB29",
        ///    [
        ///      {
        ///        "quizStartTime": "9:45",
        ///        "quizEndTime": "10:15",
        ///        "attempAllowed" : 2,
        ///        "druration":600,
        ///        },
        ///     ]
        /// </remarks>
        /// <response code="200">Trả Về Thông Báo</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpPost(ApiEndpointConstant.LectureEndPoint.SettingQuizTime)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(BadRequest))]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> SettingExam([FromBody] QuizRequest quizInfor, [FromQuery] TimeOnly quizStartTime, [FromQuery] TimeOnly quizEndTime, [FromQuery] int attemptAllowed, [FromQuery] int duration)
        {
            var settingInfor = new SettingQuizTimeRequest
            {
                QuizStartTime = quizStartTime,
                QuizEndTime = quizEndTime,
                AttemptAllowed = attemptAllowed,
                Duration = duration,
            };
            var response = await _quizService.SettingExamTimeAsync(quizInfor.ExamId, quizInfor.ClassId, settingInfor);
            return Ok(response);
        }

        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetQuizForStaff)]
        public async Task<IActionResult> GetQuizForStaff([FromRoute] string id)
        {
            var result = await _syllabusService.GetStaffQuestions(id);
            return Ok(result);
        }
        [HttpPut(ApiEndpointConstant.QuizEndPoint.UpdateQuizForStaff)]
        public async Task<IActionResult> UpdateQuizForStaff([FromRoute] string questionpackageId, UpdateQuestionPackageRequest request)
        {
            var result = await _syllabusService.UpdateQuiz(questionpackageId, request);
            if (!result)
            {
                return Ok("Update quiz gặp lỗi");
            }
            return Ok("Update quiz thành công");
        }
    }
}
