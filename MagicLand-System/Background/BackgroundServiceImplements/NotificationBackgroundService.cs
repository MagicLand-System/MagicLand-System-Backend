﻿using MagicLand_System.Background.BackgroundServiceInterfaces;
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

                        if (time >= 2 && time <= 4)
                        {
                            noti.IsRead = true;
                        }

                        if (time > 4)
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
                                    GenerateNotificationInCondition(currentDate, newNotifications, NotificationMessageContant.ChangeClassRequestTitle,
                                        NotificationMessageContant.ChangeClassRequestBody(cls.ClassCode!, stu.Student!.FullName!),
                                        currentDate.Day - cls.StartDate.Day <= 3 ? NotificationPriorityEnum.IMPORTANCE.ToString() : NotificationPriorityEnum.WARNING.ToString(), cls.Id, stu.StudentId, cls.Image!);
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

                    GenerateNotificationInCondition(currentDate, newNotifications, NotificationMessageContant.MakeUpAttendanceTitle,
                           NotificationMessageContant.MakeUpAttendanceBody(cls.ClassCode!, attendance.Student!.FullName!, schedule.Date),
                           currentDate.Day - cls.StartDate.Day <= 3 ? NotificationPriorityEnum.IMPORTANCE.ToString() : NotificationPriorityEnum.WARNING.ToString(), cls.Id, attendance.StudentId, cls.Image!);
                }
            }
        }

        private void GenerateNotificationInCondition(DateTime currentDate, List<Notification> newNotifications, string title, string body, string type, Guid classId, Guid studentId, string image)
        {
            newNotifications.Add(new Notification
            {
                Id = new Guid(),
                Title = title,
                Body = body,
                Priority = type,
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

                                GenerateNotification(currentDate, newNotifications, userId, NotificationMessageContant.LastDayRegisterTitle, NotificationMessageContant.LastDayRegisterBody(cls.ClassCode!),
                                    NotificationPriorityEnum.IMPORTANCE.ToString(), cls.Image!, actionData);
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

        private void GenerateNotification(DateTime currentDate, List<Notification> newNotifications, Guid targetUser, string title, string body, string type, string image, string actionData)
        {
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
                UserId = targetUser,
            });
        }
    }
}
