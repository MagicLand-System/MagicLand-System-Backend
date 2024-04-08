using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Courses.Custom;

namespace MagicLand_System.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<CourseWithScheduleShorten>> GetCurrentStudentCoursesAsync();
        Task<List<CourseWithScheduleShorten>> GetCoursesAsync(bool isValid);
        Task<CourseWithScheduleShorten> GetCourseByIdAsync(Guid id);
        Task<List<CourseResExtraInfor>> SearchCourseByNameOrAddedDateAsync(string keyWord);
        Task<List<CourseWithScheduleShorten>> FilterCourseAsync(int minYearsOld, int maxYearsOld, int? minNumberSession,
            int? maxNumberSession, double minPrice, double? maxPrice, string? subject, int? rate);
        Task<List<SyllabusCategory>> GetCourseCategories();
        Task<List<CourseResponseCustom>> GetCoursesOfStudentByIdAsync(Guid studentId);
        Task<bool> AddCourseInformation(CreateCourseRequest request);
        Task<StaffCourseResponse> GetStaffCourseByCourseId(string courseid);
        Task<bool> GenerateCoursePrice(CoursePriceRequest request);
        Task<List<CoursePrice>> GetCoursePrices(string courseId);
        Task<List<StaffCourseResponse>> GetCourseResponse(List<string>? categoryIds, string? searchString, int? minAge, int? MaxAge);
        Task<List<MyClassResponse>> GetClassesOfCourse(string courseId,List<string>? dateOfWeeks,string? Method,List<string>? slotId);
        Task<bool> UpdateCourse(string id, UpdateCourseRequest request);
        Task<List<CourseWithScheduleShorten>> GetCourseByStaff();
        Task<CourseSearchResponse> GetCourseSearch(string keyword);

    }
}
