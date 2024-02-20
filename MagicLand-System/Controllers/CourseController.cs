using MagicLand_System.Constants;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicLand_System.Controllers
{
    [ApiController]
    public class CourseController : BaseController<CourseController>
    {
        private readonly ICourseService _courseService;
        public CourseController(ILogger<CourseController> logger, ICourseService courseService) : base(logger)
        {
            _courseService = courseService;
        }

        #region document API Get Courses
        /// <summary>
        ///  Truy Suất Toàn Bộ Khóa Học
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Khóa Học</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _courseService.GetCoursesAsync();
            return Ok(courses);
        }

        #region document API Search Courses
        /// <summary>
        ///  Truy Suất Khóa Học Theo Tên
        /// </summary>
        /// <param name="keyWord">Từ Khóa</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "keyWord": "basic math"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Danh Sách Khóa Học</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.SearchCourse)]
        [AllowAnonymous]
        public async Task<IActionResult> SearchCourse([FromQuery] string keyWord)
        {
            List<PayLoad.Response.Courses.CourseResExtraInfor> courseResExtraInfors = await _courseService.SearchCourseByNameOrAddedDateAsync(keyWord);
            var courses = courseResExtraInfors;
            return Ok(courses);
        }


        #region document API Get Course By Id
        /// <summary>
        ///  Truy Suất Khóa Học Theo Id Của Khóa Học
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
        /// <response code="200">Trả Về Khóa Học</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.CourseById)]
        [AllowAnonymous]
        public async Task<IActionResult> GetCoureById(Guid id)
        {
            var courses = await _courseService.GetCourseByIdAsync(id);
            return Ok(courses);
        }

        #region document API Filter Courses
        /// <summary>
        /// Tìm Kiếm Hoạc Lọc Các Khóa Học Theo Tiêu Chí
        /// </summary>
        /// <param name="minYearsOld">Cho Khóa Học Có Độ Tuổi Lớn Hơn Hoặc Bằng Gía Trị Này</param>
        /// <param name="maxYearsOld">Cho Khóa Học Có Độ Tuổi Nhỏ Hơn Hoặc Bằng Gía Trị Này</param>
        /// <param name="minNumberSession">Cho Khóa Học Có Số Buổi Học Lớn Hơn Hoặc Bằng Gía Trị Này</param>
        /// <param name="maxNumberSession">Cho Khóa Học Có Số Buổi Học Nhỏ Hơn Hoặc Bằng Gía Trị Này, Nếu Để Trống Mặc Định Gía Trị Lớn Nhất</param>
        /// <param name="minPrice">Cho Khóa Học Gía Lớn Hơn Hoặc Bằng Gía Trị Này</param>
        /// <param name="maxPrice">Cho Khóa Học Gía Nhỏ Hơn Hoặc Bằng Gía Trị Này, Nếu Để Trống Mặc Định Gía Trị Lớn Nhất</param>
        /// <param name="subject">Cho Khóa Học Thuộc Lịch Vực Này</param>
        /// <param name="rate">Cho Khóa Học Có Đánh Gía Cao Hơn Hoặc Bằng Gía Trị Này</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "subject": "Math",
        ///        "minYearsOld": 3,
        ///        "maxYearsOld": 7,
        ///        "minNumberSession": 10,
        ///        "maxNumberSession": 20,
        ///        "minPrice": 2000000,
        ///        "maxPrice": 8000000,
        ///        "rate": 5
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Trả Về Khóa Học Thỏa Mãn</response>
        /// <response code="400">Yêu Cầu Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.FilterCourse)]
        [AllowAnonymous]
        public async Task<IActionResult> FilterCourse(
            [FromQuery] string subject,
            [FromQuery] int minYearsOld = 0,
            [FromQuery] int maxYearsOld = 120,
            [FromQuery] int? minNumberSession = 0,
            [FromQuery] int? maxNumberSession = null,
            [FromQuery] double minPrice = 0,
            [FromQuery] double? maxPrice = null,
            [FromQuery] int? rate = null)
        {
            var courses = await _courseService.FilterCourseAsync(minYearsOld, maxYearsOld, minNumberSession, maxNumberSession, minPrice, maxPrice, subject, rate);
            return Ok(courses);
        }
        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetCourseCategory)]
        public async Task<IActionResult> GetCourseCategory()
        {
            var categories = await _courseService.GetCourseCategories();
            return Ok(categories);
        }
        [HttpPost(ApiEndpointConstant.CourseEnpoint.AddCourse)]
        public async Task<IActionResult> InsertCourse([FromBody] CreateCourseRequest request)
        {
            var isSuccess = await _courseService.AddCourse(request);
            return Ok(isSuccess);
        }
    }
}
