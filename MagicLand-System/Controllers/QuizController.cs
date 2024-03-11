using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Customs;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class QuizController : BaseController<QuizController>
    {
        private readonly ISyllabusService _syllabusService;

        public QuizController(ILogger<QuizController> logger, ISyllabusService syllabusService) : base(logger)
        {
            _syllabusService = syllabusService;
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

        #region document API Get Exams Of Class By Class Id
        /// <summary>
        ///  Truy Suất Các Bài Kiểm Tra Của Một Lớp Học
        /// </summary>
        /// <param name="id">Id Của Lớp Học</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "id":"3c1849af-400c-43ca-979e-58c71ce9301d"  
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
        public async Task<IActionResult> GetExamOfClassByClassId([FromQuery] Guid id)
        {
            var responses = await _syllabusService.LoadExamOfClassByClassIdAsync(id);

            return Ok(responses);
        }

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
        [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuizOfExamByExamId([FromQuery] Guid id, [FromQuery] int? examPart)
        {
            var responses = await _syllabusService.LoadQuizOfExamByExamIdAsync(id, examPart);
            if (responses == default)
            {
                return Ok("Bài Kiểm Tra Này Do Giáo Viên Tự Chọn Câu Hỏi Và Đề Tài");
            }

            return Ok(responses);
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
