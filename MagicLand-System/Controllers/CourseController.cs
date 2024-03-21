﻿using MagicLand_System.Constants;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var courses = await _courseService.GetCoursesAsync(false);
            return Ok(courses);
        }
        #region document API Get Courses Statisfy
        /// <summary>
        ///  Truy Suất Các Khóa Học Có Lớp Có Thể Đăng Ký
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Khóa Học</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetAllValid)]
        [ProducesResponseType(typeof(CourseResExtraInfor), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PARENT")]
        public async Task<IActionResult> GetCoursesValid()
        {
            var courses = await _courseService.GetCoursesAsync(true);
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
            var responses = await _courseService.SearchCourseByNameOrAddedDateAsync(keyWord);
            return Ok(responses);
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

        #region document API Get Current Student Course
        /// <summary>
        ///  Truy Suất Các Khóa Học Của Học Sinh Hiện Tại
        /// </summary>
        /// <response code="200">Trả Về Danh Sách Khóa Học</response>
        /// <response code="403">Chức Vụ Không Hợp Lệ</response>
        /// <response code="500">Lỗi Hệ Thống Phát Sinh</response>
        #endregion
        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetCurrentStudentCourses)]
        [ProducesResponseType(typeof(CourseWithScheduleShorten), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> GetCurrentStudentCourses()
        {
            var courses = await _courseService.GetCurrentStudentCoursesAsync();
            return Ok(courses);
        }

        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetCourseCategory)]
        public async Task<IActionResult> GetCourseCategory()
        {
            var categories = await _courseService.GetCourseCategories();
            return Ok(categories);
        }
        [HttpPost(ApiEndpointConstant.CourseEnpoint.AddCourse)]
        public async Task<IActionResult> InsertCourseInformation([FromBody] CreateCourseRequest request)
        {
            var isSuccess = await _courseService.AddCourseInformation(request);
            return Ok(isSuccess);
        }
        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetCourseByStaff)]
        public async Task<IActionResult> GetCourseByStaff(string? id)
        {
            var isSuccess = await _courseService.GetStaffCourseByCourseId(id);
            return Ok(isSuccess);
        }
        [HttpGet(ApiEndpointConstant.CourseEnpoint.GetCoursePrice)]
        public async Task<IActionResult> GetCoursePrice(string? courseId)
        {
            var isSuccess = await _courseService.GetCoursePrices(courseId);
            return Ok(isSuccess);
        }
    }
}
