using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Repository.Implement;
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

                        if (time >= 1)
                        {
                            noti.IsRead = true;
                        }

                        if (time >= 2)
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
                       include: x => x.Include(x => x.StudentClasses).ThenInclude(sc => sc.Student)!);

                    var newNotifications = new List<Notification>();

                    foreach (var cls in classes)
                    {
                        cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                        orderBy: x => x.OrderBy(x => x.Date),
                        predicate: x => x.ClassId == cls.Id,
                        include: x => x.Include(x => x.Attendances).ThenInclude(x => x.Student).Include(x => x.Slot)!);


                        if (cls.Status == ClassStatusEnum.CANCELED.ToString())
                        {
                            foreach (var stu in cls.StudentClasses)
                            {
                                if (stu.CanChangeClass)
                                {
                                    var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                                       {
                                         ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                                         ($"{AttachValueEnum.StudentId}", $"{stu.StudentId}"),
                                       });

                                    await GenerateNotification(currentDate, newNotifications, null, NotificationMessageContant.ChangeClassRequestTitle,
                                                 NotificationMessageContant.ChangeClassRequestBody(cls.ClassCode!, stu.Student!.FullName!),
                                                 currentDate.Day - cls.StartDate.Day <= 3 ? NotificationPriorityEnum.IMPORTANCE.ToString() : NotificationPriorityEnum.WARNING.ToString(), cls.Image!, actionData, _unitOfWork);
                                }
                            }
                            continue;
                        }

                        await ForProgressingClass(currentDate, newNotifications, cls, _unitOfWork);
                    }

                    if (newNotifications.Count() > 0)
                    {
                        await _unitOfWork.GetRepository<Notification>().InsertRangeAsync(newNotifications);
                    }

                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Create New Notifications Got An Error: [{ex.Message}]";
            }
            return "Create New Notifications Success";
        }

        private async Task ForProgressingClass(DateTime currentDate, List<Notification> newNotifications, Class cls, IUnitOfWork _unitOfWork)
        {
            var CheckingSchedules = cls.Schedules.Where(sc => sc.Date.Date < currentDate.Date && sc.Attendances.Any(att => att.IsPresent == null)).ToList();
            var tempNotifications = new List<Notification>();
            int totalNonAttendance = 0;

            foreach (var schedule in CheckingSchedules)
            {
                var attendances = schedule.Attendances;
                foreach (var attendance in attendances)
                {
                    if (attendance.IsPresent != null)
                    {
                        continue;
                    }
                    totalNonAttendance++;
                    var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                        {
                          ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                          ($"{AttachValueEnum.StudentId}", $"{attendance.StudentId}"),
                        });

                    await GenerateNotification(currentDate, tempNotifications, null, NotificationMessageContant.MakeUpAttendanceTitle,
                             NotificationMessageContant.MakeUpAttendanceBody(cls.ClassCode!, attendance.Student!.FullName!, schedule.Date),
                             currentDate.Day - cls.StartDate.Day <= 3 ? NotificationPriorityEnum.IMPORTANCE.ToString() : NotificationPriorityEnum.WARNING.ToString(), cls.Image!, actionData, _unitOfWork);
                }

                if (totalNonAttendance == attendances.Count())
                {
                    var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                        {
                          ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                        });

                    await GenerateNotification(currentDate, newNotifications, cls.LecturerId, NotificationMessageContant.MakeUpAttendanceLecturerTitle,
                           NotificationMessageContant.MakeUpAttendanceLecturerBody(cls, schedule.Date, schedule.Slot!.StartTime + " - " + schedule.Slot.EndTime),
                           currentDate.Day - cls.StartDate.Day <= 3 ? NotificationPriorityEnum.IMPORTANCE.ToString() : NotificationPriorityEnum.WARNING.ToString(), cls.Image!, actionData, _unitOfWork);

                }
                else
                {
                    newNotifications.AddRange(tempNotifications);
                }
            }

        }

        public async Task<string> CreateNotificationForLastRegisterTime()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentDate = DateTime.Now;
                    var newNotifications = new List<Notification>();

                    var usersId = await _unitOfWork.GetRepository<User>().GetListAsync(selector: x => x.Id, predicate: x => x.Role!.Name == RoleEnum.PARENT.ToString());
                    foreach (var userId in usersId)
                    {
                        var items = await _unitOfWork.GetRepository<CartItem>().GetListAsync(
                            predicate: x => x.Cart!.UserId == userId && x.ClassId != default);

                        foreach (var item in items)
                        {
                            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == item.ClassId);
                            if (cls.Status != ClassStatusEnum.UPCOMING.ToString())
                            {
                                continue;
                            }

                            if (cls.StartDate.AddDays(-4).Date == currentDate.Date)
                            {
                                string actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                                {
                                  ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                                });

                                await GenerateNotification(currentDate, newNotifications, userId, NotificationMessageContant.LastDayRegisterTitle, NotificationMessageContant.LastDayRegisterBody(cls.ClassCode!),
                                       NotificationPriorityEnum.IMPORTANCE.ToString(), cls.Image!, actionData, _unitOfWork);
                            }

                            if (cls.StartDate.AddDays(-3).Date == currentDate.Date)
                            {
                                _unitOfWork.GetRepository<CartItem>().DeleteAsync(item);
                            }

                        }
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

        private async Task GenerateNotification(DateTime currentDate, List<Notification> newNotifications, Guid? targetUser, string title, string body, string type, string image, string actionData, IUnitOfWork _unitOfWork)
        {

            var listItemIdentify = new List<string>
            {
                StringHelper.TrimStringAndNoSpace(targetUser is null ? "" : targetUser.Value.ToString()),
                StringHelper.TrimStringAndNoSpace(title),
                StringHelper.TrimStringAndNoSpace(body),
                StringHelper.TrimStringAndNoSpace(image),
                StringHelper.TrimStringAndNoSpace(actionData),
            };

            string identify = StringHelper.ComputeSHA256Hash(string.Join("", listItemIdentify));
            var isNotify = await _unitOfWork.GetRepository<Notification>().SingleOrDefaultAsync(predicate: x => x.Identify == identify);
            if (isNotify != null)
            {
                return;
            }

            newNotifications.Add(new Notification
            {
                Id = new Guid(),
                Title = title,
                Body = body,
                Priority = type,
                Image = image,
                CreatedAt = currentDate,
                IsRead = false,
                ActionData = actionData,
                Identify = identify,
                UserId = targetUser,
            });
        }


        public async Task<string> CreateNotificationForRemindRegisterCourse()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentDate = DateTime.Now;
                    var newNotifications = new List<Notification>();

                    var usersId = await _unitOfWork.GetRepository<User>().GetListAsync(selector: x => x.Id, predicate: x => x.Role!.Name == RoleEnum.PARENT.ToString());
                    foreach (var userId in usersId)
                    {
                        var items = await _unitOfWork.GetRepository<CartItem>().GetListAsync(
                            predicate: x => x.Cart!.UserId == userId && x.CourseId != default);

                        foreach (var item in items)
                        {
                            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id == item.CourseId, include: x => x.Include(x => x.Classes));

                            if (course.Classes.Any(sc => sc.Status == ClassStatusEnum.UPCOMING.ToString()))
                            {
                                if (currentDate.Day % 2 == 0)
                                {
                                    string actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                                    {
                                      ($"{AttachValueEnum.CourseId}", $"{course.Id}"),
                                    });

                                    await GenerateNotification(currentDate, newNotifications, userId, NotificationMessageContant.RemindRegisterCourseTitle,
                                        NotificationMessageContant.RemindRegisterCourseBody(course.Name!),
                                   NotificationPriorityEnum.REMIND.ToString(), course.Image!, actionData, _unitOfWork);
                                }
                            }
                        }
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
    }
}
