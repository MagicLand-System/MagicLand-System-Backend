using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Student;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class StudentService : BaseService<StudentService>, IStudentService
    {
        public StudentService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<StudentService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<bool> AddStudent(CreateStudentRequest request)
        {
            if(request.DateOfBirth > DateTime.Now.AddYears(-3))
            {
                throw new BadHttpRequestException("student must be at least 3 yearolds", StatusCodes.Status400BadRequest);
            }
            var userId = (await GetUserFromJwt()).Id;
            if(request == null)
            {
                throw new BadHttpRequestException("request is invalid", StatusCodes.Status400BadRequest);
            }
            var student = _mapper.Map<Student>(request);
            student.ParentId = userId;
            await _unitOfWork.GetRepository<Student>().InsertAsync(student);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            return isSuccess;
        }

        public async Task<List<StudentClassResponse>> GetClassOfStudent(string studentId)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync( predicate : x => x.Id.ToString().Equals(studentId));  
            if(student == null)
            {
                throw new BadHttpRequestException("StudentId is not exist",StatusCodes.Status400BadRequest);
            }
            var listClassInstance = await _unitOfWork.GetRepository<ClassInstance>().GetListAsync(predicate : x => x.StudentId.ToString().Equals(studentId) , include : x => x.Include(x => x.Session));
            if (listClassInstance == null)
            {
                throw new BadHttpRequestException("Student is in not any class", StatusCodes.Status400BadRequest);
            }
            var classIds = (from classInstance in listClassInstance
                            group classInstance by classInstance.Session.ClassId into grouped
                            select new { ClassId = grouped.Key });
            Class studentClass = null;
            List<Class> classes = new List<Class>();
            var allClass = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate : x => x.Id == x.Id,include : x => x.Include(x => x.Course).Include(x => x.User));
            foreach (var classInstance in classIds)
            {
                studentClass = allClass.SingleOrDefault(x => x.Id == classInstance.ClassId);
                classes.Add(studentClass);
            }
            if(classes.Count == 0)
            {

                throw new BadHttpRequestException("Student is in not any class", StatusCodes.Status400BadRequest);
            }
            List<StudentClassResponse> responses = new List<StudentClassResponse>();
            StudentClassResponse response = null;
            foreach (var classM in classes)
            {
                response = new StudentClassResponse
                {
                    ClassName = classM.Name,
                    StatusClass = classM.Status,
                    Method = classM.Method,
                    CourseName = classM.Course.Name,
                    EndTime = classM.EndTime,
                    MaxYearOldsStudentOfCourse = classM.Course.MaxYearOldsStudent,
                    MinYearOldsStudentOfCourse = classM.Course.MinYearOldsStudent,
                    LecturerName = classM.User.FullName,
                    NumberOfSession = classM.Course.NumberOfSession,
                    StartTime = classM.StartTime,
                    Status = classM.Status
                };
                responses.Add(response);
            }
            return responses;   
        }

        public async Task<List<StudentScheduleResponse>> GetScheduleOfStudent(string studentId)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(studentId));
            if (student == null)
            {
                throw new BadHttpRequestException("StudentId is not exist", StatusCodes.Status400BadRequest);
            }
            var listClassInstance = await _unitOfWork.GetRepository<ClassInstance>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId), include: x => x.Include(x => x.Session));
            if (listClassInstance == null)
            {
                throw new BadHttpRequestException("Student is in not any class", StatusCodes.Status400BadRequest);
            }
            var sessionIds = new List<Guid>();
            foreach(var classInstance in listClassInstance)
            {
                sessionIds.Add(classInstance.Session.Id);
            }
            if(sessionIds.Count == 0) 
            {
                throw new BadHttpRequestException("Student is in not any schedule", StatusCodes.Status400BadRequest);
            }
            var sessions = await _unitOfWork.GetRepository<Session>().GetListAsync(predicate: x => x.Id == x.Id, include: x => x.Include(x => x.Class).Include(x => x.Slot).Include(x => x.Room)); 
            var listStudentSchedule = new List<StudentScheduleResponse>();
            StudentScheduleResponse studentSchedule = null;
            Session session = new Session();
            foreach (var Id in sessionIds)
            {
                session = sessions.SingleOrDefault(s => s.Id == Id);
                studentSchedule = new StudentScheduleResponse
                {
                    Date = session.Date,
                    DayOfWeek = session.DayOfWeek,
                    EndTime = session.Slot.EndTime,
                    StartTime = session.Slot.StartTime,
                    LinkURL = session.Room.LinkURL,
                    Method = session.Class.Method,
                    RoomInFloor = session.Room.Floor,
                    RoomName = session.Room.Name,
                };
                listStudentSchedule.Add(studentSchedule);
            }
            return listStudentSchedule;
        }
    }
}
