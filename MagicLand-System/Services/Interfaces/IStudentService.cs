﻿using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Student;

namespace MagicLand_System.Services.Interfaces
{
    public interface IStudentService
    {
        Task<bool> AddStudent(CreateStudentRequest request);
        Task<List<StudentClassResponse>> GetClassOfStudent(String studentId,string status);
        Task<List<StudentScheduleResponse>> GetScheduleOfStudent(string studentId);
        Task<List<Student>> GetStudentsOfCurrentParent();
    }
}
