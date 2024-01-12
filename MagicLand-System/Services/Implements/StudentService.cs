using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Student;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

        public async Task<List<StudentClassResponse>> GetClassOfStudent(string studentId,string status)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync( predicate : x => x.Id.ToString().Equals(studentId));  
            if(student == null)
            {
                throw new BadHttpRequestException("StudentId is not exist",StatusCodes.Status400BadRequest);
            }
            var listClassInstance = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId), include: x => x.Include(x => x.Class));
            if (listClassInstance == null)
            {
                return new List<StudentClassResponse>();
            }
            var classIds = (from classInstance in listClassInstance
                            group classInstance by classInstance.ClassId into grouped
                            select new { ClassId = grouped.Key });
            var myx = classIds.ToList();
            Class studentClass = null;
            List<Class> classes = new List<Class>();
            List<Class> allClass = new List<Class>();
           
            allClass = (await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.Id == x.Id, include: x => x.Include(x => x.Course).Include(x => x.Lecture))).ToList();
            foreach (var classInstance in myx)
            {
                if (status.IsNullOrEmpty()) 
                {
                    studentClass = allClass.SingleOrDefault(x => x.Id.ToString().Equals(classInstance.ClassId.ToString()));
                }
                else
                {
                    studentClass = studentClass = allClass.SingleOrDefault(x => x.Id.ToString().Equals(classInstance.ClassId.ToString()) && status.Trim().Equals(x.Status.Trim()));
                }
                if(studentClass != null)
                {
                    classes.Add(studentClass);
                }
            }
            if (classes.Count == 0)
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
                    EndTime = classM.EndDate,
                    MaxYearOldsStudentOfCourse = classM.Course.MaxYearOldsStudent.Value,
                    MinYearOldsStudentOfCourse = classM.Course.MinYearOldsStudent.Value,
                    LecturerName = classM.Lecture.FullName,
                    NumberOfSession = classM.Course.NumberOfSession,
                    StartTime = classM.StartDate,
                    Status = classM.Status
                };
                responses.Add(response);
            }
            return responses;//responses;   
        }

        public async Task<List<StudentScheduleResponse>> GetScheduleOfStudent(string studentId)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(studentId));
            if (student == null)
            {
                throw new BadHttpRequestException("StudentId is not exist", StatusCodes.Status400BadRequest);
            }
            var listClassInstance = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId), include: x => x.Include(x => x.Class));
            if (listClassInstance == null)
            {
                return new List<StudentScheduleResponse>();
            }
            var sessionIds = new List<Guid>();
            foreach (var classInstance in listClassInstance)
            {
                sessionIds.Add(classInstance.Class.Id);
            }
            if (sessionIds.Count == 0)
            {
                //throw new BadHttpRequestException("Student is in not any schedule", StatusCodes.Status400BadRequest);
                return new List<StudentScheduleResponse>();
            }
            var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.Id == x.Id, include: x => x.Include(x => x.Class).Include(x => x.Slot).Include(x => x.Room).Include(x => x.Class));
            var listStudentSchedule = new List<StudentScheduleResponse>();
            StudentScheduleResponse studentSchedule = null;
            List<Schedule> scheduleList = new List<Schedule>();
            foreach (var Id in sessionIds)
            {
                var listResult = (schedules.Where(s => s.ClassId == Id)).ToList();
                scheduleList.AddRange(listResult);
            }
            foreach (var schedule in scheduleList)
            {
                var lecturerName = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(selector: x => x.FullName, predicate: x => x.Id.Equals(schedule.Class.LecturerId));
                studentSchedule = new StudentScheduleResponse
                {
                    StudentName = student.FullName,
                    Date = schedule.Date,
                    DayOfWeek = schedule.DayOfWeek,
                    EndTime = schedule.Slot.EndTime,
                    StartTime = schedule.Slot.StartTime,
                    LinkURL =  schedule.Room.LinkURL,
                    Method = schedule.Class.Method,
                    RoomInFloor = schedule.Room.Floor,
                    RoomName = schedule.Room.Name,
                    ClassName = schedule.Class.Name,
                    LecturerName = lecturerName,
                };
                listStudentSchedule.Add(studentSchedule);
            }
            return listStudentSchedule; //listStudentSchedule;
        }

        public async Task<List<Student>> GetStudentsOfCurrentParent()
        {
            var students = await _unitOfWork.GetRepository<Student>().GetListAsync(predicate: x => x.ParentId == GetUserIdFromJwt());
            return students.ToList();
        }
    }
}
