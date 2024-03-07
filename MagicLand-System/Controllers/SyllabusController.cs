using MagicLand_System.Constants;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Request.Syllabus;
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

        [HttpPost(ApiEndpointConstant.SyllabusEndPoint.AddSyllabus)]
        public async Task<IActionResult> InsertCourse([FromBody] OverallSyllabusRequest request)
        {
            var isSuccess = await _syllabusService.AddSyllabus(request);
            string message = "Tạo Giáo Trình Sảy Ra Lỗi";
            if (isSuccess)
            {
                message = "Tạo Giáo Trình Thành Công";
            }
            return Ok(message);
        }

        #region document API Get Syllabus By Item Id
        /// <summary>
        ///  Truy Suất Giáo Trình Thông Qua Id Khóa Học, Hoặc Truy Suất Giáo Trình Cùng Lịch Học Của Lớp
        /// </summary>
        /// <param name="courseId">Id Của Khóa Học</param>
        /// <param name="classId">Id Của Lớp Học (Option)</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "courseId": "c6d70a5f-56ae-4de0-b441-c080da024524",
        ///        "classId": "1a5fg4a9-dd66-h10a-1agh-dd70da024a47"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Giáo Trình</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.LoadByCourse)]
        [ProducesResponseType(typeof(SyllabusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SyllabusWithScheduleResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> LoadSyllabusByCourseId([FromQuery] Guid courseId, [FromQuery] Guid classId)
        {
            (SyllabusResponse?, SyllabusWithScheduleResponse?) response = await _syllabusService.LoadSyllabusByCourseIdAsync(courseId, classId);

            if(response.Item1 != default)
            {
                return Ok(response.Item1);
            }

            return Ok(response.Item2);
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
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.LoadSyllabus)]
        [ProducesResponseType(typeof((SyllabusResponse, SyllabusWithCourseResponse)), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [AllowAnonymous]
        public async Task<IActionResult> LoadSyllabusById([FromQuery] Guid id)
        {
            (SyllabusResponse?, SyllabusWithCourseResponse?) response = await _syllabusService.LoadSyllabusByIdAsync(id);

            if (response.Item1 != default)
            {
                return Ok(response.Item1);
            }

            return Ok(response.Item2);
        }

        #region document API Get Syllabuses
        /// <summary>
        ///  Truy Suất Toàn Bộ Giáo Trình
        /// </summary>
        /// <response code="200">Trả Về Giáo Trình</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.LoadSyllabuses)]
        [ProducesResponseType(typeof(SyllabusResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize]
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
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.FilterSyllabus)]
        [ProducesResponseType(typeof(SyllabusResponse), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(Exception))]
        [Authorize]
        public async Task<IActionResult> FilterSyllabus([FromQuery] List<string>? keyWords,
            [FromQuery] DateTime? date,
            [FromQuery] double? score)
        {
            var syllabuses = await _syllabusService.FilterSyllabusAsync(keyWords, date, score);
            return Ok(syllabuses);
        }
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.GeneralSyllabus)]
        public async Task<IActionResult> GetAll(string? keyword)
        {
            var course = await _syllabusService.GetAllSyllabus(keyword);
            return Ok(course);
        }
        [HttpPut(ApiEndpointConstant.SyllabusEndPoint.UpdateSyllabus)]
        public async Task<IActionResult> UpdateSyll([FromRoute] string id , OverallSyllabusRequest request)
        {
            var isSuccess = await _syllabusService.UpdateSyllabus(request,id);
            string message = "Cập Nhật Giáo Trình Sảy Ra Lỗi";
            if (isSuccess)
            {
                message = "Cập Nhật Giáo Trình Thành Công";
            }
            return Ok(message);
        }
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.StaffSyl)]
        public async Task<IActionResult> GetStaffSyllabus([FromRoute] string id)
        {
            var result = await _syllabusService.GetStaffSyllabusResponse(id);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.AvailableSyl)]
        public async Task<IActionResult> GetAvailableSyllabus(string? keyword = null)
        {
            var result = await _syllabusService.GetStaffSyllabusCanInsert(keyword);
            return Ok(result);
        }
        [HttpPut(ApiEndpointConstant.SyllabusEndPoint.UpdateOverall)]
        public async Task<IActionResult> UpdateOverall([FromRoute] string id , UpdateOverallSyllabus request)
        {
            var result = await _syllabusService.UpdateOverallSyllabus(id , request);
            if (!result)
            {
                return BadRequest("Update failed");
            }
            return Ok("Update Success");
        }
        [HttpPut(ApiEndpointConstant.SyllabusEndPoint.UpdateTopic)]
        public async Task<IActionResult> UpdateTopic([FromRoute] string topicId, UpdateTopicRequest request)
        {
            var result = await _syllabusService.UpdateTopic(topicId, request);
            if (!result)
            {
                return BadRequest("Update failed");
            }
            return Ok("Update Success");
        }
        //[HttpPut(ApiEndpointConstant.SyllabusEndPoint.UpdateSession)]
        //public async Task<IActionResult> UpdateSession([FromRoute] string descriptionId, UpdateSessionRequest request)
        //{
        //    var result = await _syllabusService.UpdateSession(descriptionId, request);
        //    if (!result)
        //    {
        //        return BadRequest("Update failed");
        //    }
        //    return Ok("Update Success");
        //}
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.GenralInfromation)]
        public async Task<IActionResult> GetStaffGeneralSyllabus(string id)
        {
            var result = await _syllabusService.GetGeneralSyllabusResponse(id);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.MaterialInfor)]
        public async Task<IActionResult> GetStaffMaterialSyllabus(string id)
        {
            var result = await _syllabusService.GetMaterialResponse(id);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.ExamSyllabus)]
        public async Task<IActionResult> GetStaffExamSyllabus(string id)
        {
            var result = await _syllabusService.GetStaffExamSyllabusResponses(id);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.SessionSyllabus)]
        public async Task<IActionResult> GetStaffSessionSyllabus(string id)
        {
            var result = await _syllabusService.GetAllSessionResponses(id);
            return Ok(result);
        }
        [HttpGet(ApiEndpointConstant.SyllabusEndPoint.QuestionSyllabus)]
        public async Task<IActionResult> GetStaffQuestionSyllabus(string id)
        {
            var result = await _syllabusService.GetStaffQuestionPackageResponses(id);
            return Ok(result);
        }
    }
}
