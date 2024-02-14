using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Background.BackgroundServiceImplements
{
    public class NotificationBackgroundService : INotificationBackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public NotificationBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<string> ModifyNotificationAfterTime()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentDate = DateTime.Now;

                    var notifications = await _unitOfWork.GetRepository<Notification>()
                     .GetListAsync(predicate: x => x.IsRead == false);

                    foreach (var noti in notifications)
                    {
                        int time = currentDate.Day - noti.CreatedAt.Day;

                        if (time >= 5 && time <= 10)
                        {
                            noti.IsRead = true;
                        }

                        if (time > 10)
                        {
                            _unitOfWork.GetRepository<Notification>().DeleteAsync(noti);
                        }
                    }

                    _unitOfWork.GetRepository<Notification>().UpdateRange(notifications);
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Push Notifications Got An Error: [{ex.Message}]";
            }
            return "Push Notifications Success";
        }
        public async Task<string> PushNotificationRealTime()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentDate = DateTime.Now;

                    //   await _unitOfWork.GetRepository<Notification>().InsertRangeAsync(newNotifications);
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Push Notifications Got An Error: [{ex.Message}]";
            }
            return "Push Notifications Success";
        }
        public async Task<string> CreateNewNotificationInCondition()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentDate = DateTime.Now;

                    var classes = await _unitOfWork.GetRepository<Class>()
                      .GetListAsync(predicate: x => x.Status == ClassStatusEnum.CANCELED.ToString() || x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                       include: x => x.Include(x => x.StudentClasses).ThenInclude(sc => sc.Student)
                       .Include(x => x.Schedules).ThenInclude(sc => sc.Attendances).ThenInclude(att => att.Student!));

                    var newNotifications = new List<Notification>();

                    foreach (var cls in classes)
                    {
                        if (cls.Status == ClassStatusEnum.CANCELED.ToString())
                        {
                            foreach (var stu in cls.StudentClasses)
                            {
                                if (stu.CanChangeClass)
                                {
                                    GenerateNotification(currentDate, newNotifications, NotificationMessageContant.ChangeClassTitle,
                                        NotificationMessageContant.ChangeClassBody(cls.ClassCode!, stu.Student!.FullName!),
                                        currentDate.Day - cls.StartDate.Day <= 3 ? NotificationTypeEnum.IMPORTANCE.ToString() : NotificationTypeEnum.WARNING.ToString(), cls.Id, stu.StudentId, cls.Image!);
                                }
                            }
                            continue;
                        }

                        ForProgressingClass(currentDate, newNotifications, cls);
                    }

                    await _unitOfWork.GetRepository<Notification>().InsertRangeAsync(newNotifications);
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Create New Notifications Got An Error: [{ex.Message}]";
            }
            return "Create New Notifications Success";
        }

        private void ForProgressingClass(DateTime currentDate, List<Notification> newNotifications, Class cls)
        {
            var CheckingSchedules = cls.Schedules.Where(sc => sc.Date.Date < currentDate.Date && sc.Attendances.Any(att => att.IsPresent == null)).ToList();

            foreach (var schedule in CheckingSchedules)
            {
                foreach (var attendance in schedule.Attendances)
                {
                    if (attendance.IsPresent != null)
                    {
                        continue;
                    }

                    GenerateNotification(currentDate, newNotifications, NotificationMessageContant.MakeUpAttendanceTitle,
                           NotificationMessageContant.MakeUpAttendanceBody(cls.ClassCode!, attendance.Student!.FullName!, schedule.Date),
                           currentDate.Day - cls.StartDate.Day <= 3 ? NotificationTypeEnum.IMPORTANCE.ToString() : NotificationTypeEnum.WARNING.ToString(), cls.Id, attendance.StudentId, cls.Image!);
                }
            }
        }

        private void GenerateNotification(DateTime currentDate, List<Notification> newNotifications, string title, string body, string type, Guid classId, Guid studentId, string image)
        {
            newNotifications.Add(new Notification
            {
                Id = new Guid(),
                Title = title,
                Body = body,
                Type = type,
                Image = image,
                CreatedAt = currentDate,
                IsRead = false,
                ActionData = StringHelper.GenerateJsonString(new List<(string, string)>
                {
                 ($"{AttachValueEnum.ClassId}", $"{classId}"),
                 ($"{AttachValueEnum.StudentId}", $"{studentId}"),
                })
            });
        }
    }
}
