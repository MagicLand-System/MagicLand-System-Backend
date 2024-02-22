using MagicLand_System.Constants;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{

    [ApiController]
    public class SyllabusController : BaseController<SyllabusController>
    {
        private readonly ISyllabusService _syllabusService;
        public SyllabusController(ILogger<SyllabusController> logger, ISyllabusService syllabusService) : base(logger)
        {
            _logger = logger;
            _syllabusService = syllabusService;
        }

        [HttpPost(ApiEndpointConstant.Syllabus.AddSyllabus)]
        public async Task<IActionResult> InsertCourse([FromBody] OverallSyllabusRequest request)
        {
            var isSuccess = await _syllabusService.AddSyllabus(request);
            return Ok(isSuccess);
        }

        #region document API Get Syllabus By Course Id
        /// <summary>
        ///  Truy Suất Giáo Trình Thông Qua Id Của Khóa Học
        /// </summary>
        /// <param name="id">Id Của Khóa Học</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "Id": "c6d70a5f-56ae-4de0-b441-c080da024524"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Giáo Trình</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.Syllabus.LoadByCourse)]
        [ProducesResponseType(typeof(SyllabusResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        public async Task<IActionResult> LoadSyllabusByCourseId([FromQuery] Guid id)
        {
            var syllabus = await _syllabusService.LoadSyllabusByCourseIdAsync(id);
            return Ok(syllabus);
        }

        #region document API Get Syllabus By Id
        /// <summary>
        ///  Truy Suất Giáo Trình Thông Qua Id
        /// </summary>
        /// <param name="id">Id Của Giáo Trình Cần Truy Suất</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "Id": "c6d70a5f-56ae-4de0-b441-c080da024524"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Giáo Trình</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.Syllabus.LoadSyllabus)]
        [ProducesResponseType(typeof(SyllabusResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> LoadSyllabusById([FromQuery] Guid id)
        {
            var syllabus = await _syllabusService.LoadSyllabusByIdAsync(id);
            return Ok(syllabus);
        }

        #region document API Get Syllabuses
        /// <summary>
        ///  Truy Suất Toàn Bộ Giáo Trình
        /// </summary>
        /// <response code="200">Trả Về Giáo Trình</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.Syllabus.LoadSyllabuses)]
        [ProducesResponseType(typeof(SyllabusResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> LoadSyllabuses()
        {
            var syllabuses = await _syllabusService.LoadSyllabusesAsync();
            return Ok(syllabuses);
        }

        #region document API Filter Syllabus
        /// <summary>
        ///  Tìm Kiếm Hoặc Lọc Giáo Trình Thông Qua Các Nhãn Cho Phép
        /// </summary>
        /// <param name="keyWords">Cho Giáo Trình Thỏa Mãn 1 Trong Các Từ Khóa</param>
        /// <param name="date">Cho Giáo Trình Có Biến Ngày Tháng Bằng Giá Trị Này</param>
        /// <param name="score">Cho Giáo Trình Có Biến Điểm Số Lơn Hơn Hoặc Bằng Giá Trị Này</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "keyWords": ["Toán tư duy cho bé", "TTD1"],
        ///        "date": "2024-02-22",
        ///        "score": 5.5,
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Giáo Trình</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.Syllabus.FilterSyllabus)]
        [ProducesResponseType(typeof(SyllabusResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> FilterSyllabus([FromQuery] List<string>? keyWords,
            [FromQuery] DateTime? date,
            [FromQuery] double? score)
        {
            var syllabuses = await _syllabusService.FilterSyllabusAsync(keyWords, date, score);
            return Ok(syllabuses);
        }
        [HttpGet(ApiEndpointConstant.Syllabus.GetAll)]
        public async Task<IActionResult> GetAll(string? keyword)
        {
            var course = await _syllabusService.GetAllSyllabus(keyword);
            return Ok(course);
        }
    }
}
