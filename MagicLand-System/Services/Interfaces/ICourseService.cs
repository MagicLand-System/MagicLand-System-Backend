﻿using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Course;
using MagicLand_System.PayLoad.Response.Courses;

namespace MagicLand_System.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<CourseResExtraInfor>> GetCurrentStudentCoursesAsync();
        Task<List<CourseResExtraInfor>> GetCoursesAsync(bool isValid);
        Task<CourseResExtraInfor> GetCourseByIdAsync(Guid id);
        Task<List<CourseResExtraInfor>> SearchCourseByNameOrAddedDateAsync(string keyWord);
        Task<List<CourseResExtraInfor>> FilterCourseAsync(int minYearsOld, int maxYearsOld, int? minNumberSession,
            int? maxNumberSession, double minPrice, double? maxPrice, string? subject, int? rate);
        Task<List<SyllabusCategory>> GetCourseCategories();
        Task<List<CourseResExtraInfor>> GetCoursesOfStudentByIdAsync(Guid studentId);
        Task<bool> AddCourseInformation(CreateCourseRequest request);
        Task<StaffCourseResponse> GetStaffCourseByCourseId(string courseid);
    }
}
