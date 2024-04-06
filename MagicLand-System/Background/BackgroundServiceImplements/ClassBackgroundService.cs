using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Background.BackgroundServiceImplements
{
    public class ClassBackgroundService : IClassBackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ClassBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task<string> UpdateClassInTimeAsync()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentDate = DateTime.Now;

                    var classes = await _unitOfWork.GetRepository<Class>()
                       .GetListAsync(predicate: x => x.Status != ClassStatusEnum.CANCELED.ToString() && x.Status != ClassStatusEnum.COMPLETED.ToString(),
                            include: x => x.Include(x => x.StudentClasses).Include(x => x.Schedules).ThenInclude(sc => sc.Attendances));

                    foreach (var cls in classes)
                    {
                        CheckingDateTime(cls, currentDate);
                    }

                    _unitOfWork.GetRepository<Class>().UpdateRange(classes);
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Updating Classes Got An Error: [{ex.Message}]";
            }
            return "Updating Classes Success";
        }

        private void CheckingDateTime(Class cls, DateTime currentDate)
        {
            if (cls.StartDate.Date == currentDate.AddDays(3).Date)
            {
                UpdateStudent(cls, ClassStatusEnum.LOCKED.ToString());
                return;
            }

            if (cls.StartDate.Date == currentDate.Date)
            {
                //if (cls.StudentClasses.Count() < cls.LeastNumberStudent)
                //{
                //    UpdateAttendance(cls, ClassStatusEnum.CANCELED.ToString());
                //    return;
                //}

                UpdateStudent(cls, ClassStatusEnum.PROGRESSING.ToString());
                UpdateAttendance(cls, true);
                return;
            }

            if (cls.EndDate.Date == currentDate.AddDays(-1).Date)
            {
                UpdateStudent(cls, ClassStatusEnum.COMPLETED.ToString());
                UpdateAttendance(cls, false);
            }
        }

        private void UpdateStudent(Class cls, string classStatus)
        {
            cls.Status = classStatus;
            cls.StudentClasses.ToList().ForEach(stu => stu.CanChangeClass = false);
        }

        private void UpdateAttendance(Class cls, bool attendanceStatus)
        {
            var schedules = cls.Schedules;

            foreach (var schedule in schedules)
            {
                schedule.Attendances.ToList().ForEach(att => att.IsValid = attendanceStatus);
            }
        }
    }
}
