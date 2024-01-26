using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Response.Attendances;
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
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Attendances.Where(att => att.IsPublic == true)).ThenInclude(att => att.Student)!);

            if (classes == null)
            {
                throw new BadHttpRequestException($"Các Lớp Của Giao Viên Hiện Chưa Diễn Ra Hoặc Giao Viên Chưa Được Phân Công Dạy", StatusCodes.Status400BadRequest);
            }

            return classes.Select(x => _mapper.Map<AttendanceWithClassResponse>(x)).ToList();
        }

        public async Task<List<AttendanceWithClassResponse>> GetAttendanceOfClassStudent(Guid id)
        {
            var classes = await _unitOfWork.GetRepository<Class>()
                .GetListAsync(predicate: x => x.StudentClasses.Any(sc => sc.StudentId == id) && x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                include: x => x.Include(x => x.Lecture)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Room)
                .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Attendances.Where(att => att.IsPublic == true)).ThenInclude(att => att.Student)!);

            if (classes == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Học Sinh Không Tồn Tại Hoặc Học Sinh Vẫn Chưa Có Lịch Học Diễn Ra", StatusCodes.Status400BadRequest);
            }

            return classes.Select(x => _mapper.Map<AttendanceWithClassResponse>(x)).ToList();
        }
    }
}
