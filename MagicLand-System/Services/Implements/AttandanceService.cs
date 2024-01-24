using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request.Attendance;
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

        public async Task<List<StaffAttandaceResponse>> LoadAttandance(string scheduleId,string? searchString)
        {
            var schedule = await _unitOfWork.GetRepository<Schedule>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(scheduleId),include : x => x.Include(x => x.Class));
            if (schedule == null)
            {
                return new List<StaffAttandaceResponse>();
            }
            var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(schedule.ClassId.ToString()), include: x => x.Include(x => x.StudentClasses).Include(x => x.Course).ThenInclude(x => x.CourseCategory));
            if (classx == null)
            {
                return new List<StaffAttandaceResponse>();
            }
            List<StaffAttandaceResponse> responses = new List<StaffAttandaceResponse>();
             var attandances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.ScheduleId.ToString().Equals(schedule.Id.ToString()), include: x => x.Include(x => x.Student).ThenInclude(x => x.User));
                foreach (var attendance in attandances)
                {
                    var isPresent = false;
                    if (attendance.IsPresent != null)
                    {
                        if (attendance.IsPresent.Value == true)
                        {
                            isPresent = true;
                        }
                        if(attendance.IsPresent.Value == false)
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
                        Day = schedule.Date,
                        IsPresent = isPresent,
                        Student = attendance.Student,

                    };
                    responses.Add(att);
                }
            if(searchString != null)
            {
                responses = responses.Where(x => (x.Student.FullName.Trim().ToLower().Contains(searchString.ToLower().Trim()) || x.Student.User.Phone.Trim().Equals(searchString))).ToList();
            }
            
            return responses;
        }

        public async Task<bool> TakeAttandance(List<StaffClassAttandanceRequest> requests)
        {
            if(requests  == null || requests.Count == 0) return false;
            foreach(var request in requests)
            {
                var attanadance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate : x => x.Id.ToString().Equals(request.Id.ToString()));
                if(attanadance == null) return false;
                attanadance.IsPresent = request.IsPresent;
                _unitOfWork.GetRepository<Attendance>().UpdateAsync(attanadance);
                bool isSuccess = await _unitOfWork.CommitAsync() > 0;
                if(!isSuccess)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
