using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Customs;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        #region document API get Quizzes
        /// <summary>
        ///  Truy Suất Toàn Bộ Quiz Của Các Khóa Học Đã Có Giáo Trình
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Quiz</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.EndPointBase)]
        [ProducesResponseType(typeof(QuizResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuizzes()
        {
            var responses = await _syllabusService.LoadQuizzesAsync();

            return Ok(responses);
        }

        #region document API Get Quizzes By Course Id
        /// <summary>
        ///  Truy Suất Quiz Của Một Khóa Học Dựa Vào Id Của Khóa
        /// </summary>
        /// <param name="id">Id Của Khóa Học</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "id":"3c1849af-400c-43ca-979e-58c71ce9301d"  
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Quiz Của Khóa Học</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetQuizByCourseId)]
        [ProducesResponseType(typeof(QuizResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuizByCourseId([FromRoute] Guid id)
        {
            var responses = await _syllabusService.LoadQuizzesByCourseIdAsync(id);

            return Ok(responses);
        }

        #region document API Get Quizzes By Class Id
        /// <summary>
        ///  Truy Suất Quiz Và Ngày Làm Dựa Vào Id Của Lớp
        /// </summary>
        /// <param name="id">Id Của Lớp Học</param>
        /// <remarks>
        /// Sample request:
        ///{     
        ///    "id":"3c1849af-400c-43ca-979e-58c71ce9301d"  
        ///}
        /// </remarks>
        /// <response code="200">Trả Về Quiz Của Khóa Học</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.QuizEndPoint.GetQuizByClassId)]
        [ProducesResponseType(typeof(QuizResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuizByClassId([FromRoute] Guid id)
        {
            var responses = await _syllabusService.LoadQuizzesByClassIdAsync(id);

            return Ok(responses);
        }
    }
}
