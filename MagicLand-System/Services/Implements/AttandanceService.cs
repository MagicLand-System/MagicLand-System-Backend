using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class AttandanceService : BaseService<AttandanceService>, IAttandanceService
    {
        public AttandanceService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<AttandanceService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<StaffAttandaceResponse>> LoadAttandance(string classId, DateTime day)
        {
            var schedule = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => (x.ClassId.ToString().Equals(classId) && x.Date.Year == day.Date.Year && x.Date.Month == day.Date.Month && x.Date.Day == day.Date.Day));
            if (schedule == null)
            {
                return new List<StaffAttandaceResponse>();
            }
            var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId), include: x => x.Include(x => x.StudentClasses).Include(x => x.Course).ThenInclude(x => x.CourseCategory));
            if (classx == null)
            {
                return new List<StaffAttandaceResponse>();
            }
            List<StaffAttandaceResponse> responses = new List<StaffAttandaceResponse>();
            foreach (var record in schedule)
            {
                var attandances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.ScheduleId.ToString().Equals(record.Id.ToString()), include: x => x.Include(x => x.Student));
                foreach (var attendance in attandances)
                {
                    var isPresent = "Not Yet";
                    if (attendance.IsPresent != null)
                    {
                        if (attendance.IsPresent.Value == true)
                        {
                            isPresent = "Attended";
                        }
                        isPresent = "Absent";
                    }
                    StaffAttandaceResponse att = new StaffAttandaceResponse
                    {
                        Id = record.Id,
                        Class = new PayLoad.Response.Classes.ClassResponse
                        {
                            ClassCode = classx.ClassCode,
                            ClassId = classx.Id,
                            CoursePrice = classx.Course.Price,
                            ClassSubject = classx.Course.CourseCategory.Name,
                            LeastNumberStudent = classx.LeastNumberStudent,
                            LimitNumberStudent = classx.LimitNumberStudent,
                            Image = classx.Image,
                            Name = classx.Name,
                            EndDate = classx.EndDate,
                            Method = classx.Method,
                            StartDate = classx.StartDate,
                            CourseId = classx.CourseId,
                            NumberStudentRegistered = classx.StudentClasses.Count(),
                            Status = classx.Status,
                        },
                        Day = record.Date,
                        IsPresent = isPresent,
                        Student = attendance.Student,

                    };
                    responses.Add(att);
                }
            }
            return responses;
        }
    }
}
