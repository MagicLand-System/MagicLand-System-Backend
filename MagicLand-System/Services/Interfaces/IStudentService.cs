﻿using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Request.Evaluates;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Evaluates;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Student;
using MagicLand_System.PayLoad.Response.Schedules.ForStudent;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.Services.Interfaces
{
    public interface IStudentService
    {
        Task<List<QuizResultWithStudentWork>> GetStudentQuizFullyInforAsync(Guid classId, List<Guid>? studentIdList, List<Guid>? examIdList, bool isLatestAttempt);
        Task<List<ScheduleReLearn>> FindValidDayReLearningAsync(Guid studentId, Guid classId, List<DateOnly> dayOffs);
        Task<List<StudentLearningProgress>> GetStudentLearningProgressAsync(Guid studentId, Guid classId);
        Task<AccountStudentResponse> AddStudent(CreateStudentRequest request);
        Task<List<ClassWithSlotShorten>> GetClassOfStudent(String studentId, string status);
        Task<List<AccountStudentResponse>> GetStudentAccountAsync(Guid? id);
        Task<List<StudentScheduleResponse>> GetScheduleOfStudent(string studentId);
        Task<List<Student>> GetStudentsOfCurrentParent();
        Task<StudentResponse> UpdateStudentAsync(UpdateStudentRequest newStudentInfor, Student oldStudentInfor);
        Task<string> DeleteStudentAsync(Student student);
        Task<string> TakeStudentAttendanceAsync(AttendanceRequest request, SlotEnum slot);
        Task<string> EvaluateStudentAsync(EvaluateRequest request, int noSession);
        Task<List<EvaluateResponse>> GetStudentEvaluatesAsync(Guid classId, int? noSession);
        Task<List<AttendanceResponse>> GetStudentAttendanceFromClassInNow(Guid classId);
        Task<StudentResponse> GetStudentById(Guid id);
        Task<List<StudentStatisticResponse>> GetStatisticNewStudentRegisterAsync(PeriodTimeEnum time);
    }
}
