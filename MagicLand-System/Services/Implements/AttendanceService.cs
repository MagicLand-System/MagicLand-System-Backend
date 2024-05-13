using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class AttendanceService : BaseService<AttendanceService>, IAttendanceService
    {
        public AttendanceService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<AttendanceService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }
        public async Task<List<StaffAttandaceResponse>> LoadAttandance(string scheduleId, string? searchString)
        {
            var schedule = await _unitOfWork.GetRepository<Schedule>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(scheduleId), include: x => x.Include(x => x.Class));
            if (schedule == null)
            {
                return new List<StaffAttandaceResponse>();
            }
            var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(schedule.ClassId.ToString()), include: x => x.Include(x => x.StudentClasses).Include(x => x.Course));
            //.ThenInclude(x => x.CourseCategory));
            if (classx == null)
            {
                return new List<StaffAttandaceResponse>();
            }
            List<StaffAttandaceResponse> responses = new List<StaffAttandaceResponse>();
            var attandances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.ScheduleId.ToString().Equals(schedule.Id.ToString()), include: x => x.Include(x => x.Student).ThenInclude(x => x.User));
            var studentclassCount = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == classx.Id && x.Status.Equals("Saved"));
            foreach (var attendance in attandances)
            {
                var studentclass = await _unitOfWork.GetRepository<StudentClass>().SingleOrDefaultAsync(predicate: x => x.StudentId == attendance.StudentId && x.ClassId == classx.Id);
                if (attendance.Note.Equals("CanNotMakeUp"))
                {
                    continue;
                }
                if (studentclass.Status.Equals("Saved"))
                {
                    continue;
                }
                var isPresent = false;
                if (attendance.IsPresent != null)
                {
                    if (attendance.IsPresent.Value == true)
                    {
                        isPresent = true;
                    }
                    if (attendance.IsPresent.Value == false)
                    {
                        isPresent = false;

                    }
                }
                StaffAttandaceResponse att = new StaffAttandaceResponse
                {
                    Id = attendance.Id,
                    Class = new PayLoad.Response.Classes.ClassResponse
                    {
                        ClassCode = classx.ClassCode,
                        ClassId = classx.Id,
                        //CoursePrice = classx.Course.Price,
                        //ClassSubject = classx.Course.CourseCategory.Name,
                        LeastNumberStudent = classx.LeastNumberStudent,
                        LimitNumberStudent = classx.LimitNumberStudent,
                        Image = classx.Image,
                        EndDate = classx.EndDate,
                        Method = classx.Method,
                        StartDate = classx.StartDate,
                        CourseId = classx.CourseId,
                        NumberStudentRegistered = classx.StudentClasses.Count() - studentclassCount.Count,
                        Status = classx.Status,
                    },
                    Day = schedule.Date,
                    IsPresent = isPresent,
                    Student = attendance.Student,

                };
                responses.Add(att);
            }
            if (searchString != null)
            {
                responses = responses.Where(x => (x.Student.FullName.Trim().ToLower().Contains(searchString.ToLower().Trim()) || x.Student.User.Phone.Trim().Equals(searchString))).ToList();
            }

            return responses;
        }

        public async Task<bool> TakeAttandance(List<StaffClassAttandanceRequest> requests)
        {
            if (requests == null || requests.Count == 0) return false;
            foreach (var request in requests)
            {
                var attanadance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(request.Id.ToString()));
                if (attanadance == null) return false;
                attanadance.IsPresent = request.IsPresent;
                _unitOfWork.GetRepository<Attendance>().UpdateAsync(attanadance);
                bool isSuccess = await _unitOfWork.CommitAsync() > 0;
                if (!isSuccess)
                {
                    return false;
                }
            }
            return true;
        }
        public async Task<AttendanceWithClassResponse> GetAttendanceOfClassAsync(Guid id)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == id && x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                include: x => x.Include(x => x.Lecture)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Room)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Attendances.Where(att => att.IsPublic == true)).ThenInclude(att => att.Student)!);

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Lớp Học Không Tồn Tại Hoặc Lớp Học/Lịch Học Của Học Sinh Chưa Bắt Đầu", StatusCodes.Status400BadRequest);
            }

            return _mapper.Map<AttendanceWithClassResponse>(cls);
        }

        public async Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassesAsync()
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                include: x => x.Include(x => x.Lecture)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Room)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Attendances.Where(att => att.IsPublic == true)).ThenInclude(att => att.Student)!);

            return classes.Select(x => _mapper.Map<AttendanceWithClassResponse>(x)).ToList();

        }

        public async Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassesOfCurrentUserAsync()
        {
            var userId = GetUserIdFromJwt();

            var classes = await _unitOfWork.GetRepository<Class>()
                 .GetListAsync(predicate: x => x.LecturerId == userId && x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                 include: x => x.Include(x => x.Lecture)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Room)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)));

            if (classes == null)
            {
                throw new BadHttpRequestException($"Các Lớp Của Giao Viên Hiện Chưa Diễn Ra Hoặc Giao Viên Chưa Được Phân Công Dạy", StatusCodes.Status400BadRequest);
            }

            foreach (var cls in classes)
            {
                foreach (var sch in cls.Schedules)
                {
                    sch.Attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(
                        predicate: x => x.Id == sch.Id && x.Student!.StudentClasses.Any(sc => sc.ClassId == cls.Id && sc.SavedTime == null),
                        include: x => x.Include(x => x.Student)!);
                }
            }

            return classes.Select(x => _mapper.Map<AttendanceWithClassResponse>(x)).ToList();
        }

        public async Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassStudent(Guid id)
        {
            var classes = await _unitOfWork.GetRepository<Class>()
                .GetListAsync(predicate: x => x.StudentClasses.Any(sc => sc.StudentId == id && sc.SavedTime == null) && x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                include: x => x.Include(x => x.Lecture)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Room)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Attendances.Where(att => att.IsPublic == true)).ThenInclude(att => att.Student)!);

            if (classes == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Học Sinh Không Tồn Tại, Học Sinh Chưa Tham Gia Lớp Học Nào Hoặc Học Sinh Đã Bảo Lưu Lớp Đang Học", StatusCodes.Status400BadRequest);
            }

            return classes.Select(x => _mapper.Map<AttendanceWithClassResponse>(x)).ToList();
        }
    }
}
