using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace MagicLand_System.Background.BackgroundServiceImplements
{
    public class NotificationBackgroundService : INotificationBackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public NotificationBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task<string> CreateNewNotificationInCondition()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentTime = BackgoundTime.GetTime();

                    var classes = await _unitOfWork.GetRepository<Class>()
                      .GetListAsync(predicate: x => x.Status == ClassStatusEnum.CANCELED.ToString() || x.Status == ClassStatusEnum.PROGRESSING.ToString() || x.Status == ClassStatusEnum.UPCOMING.ToString(),
                       include: x => x.Include(x => x.StudentClasses).ThenInclude(sc => sc.Student)!);

                    var newNotifications = new List<Notification>();

                    newNotifications.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Tạo Lúc  " + currentTime,
                    });

                    foreach (var cls in classes)
                    {
                        if (cls.Status == ClassStatusEnum.UPCOMING.ToString())
                        {
                            await CheckingUpComingClass(_unitOfWork, currentTime, newNotifications, cls);
                            continue;
                        }

                        cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                        orderBy: x => x.OrderBy(x => x.Date),
                        predicate: x => x.ClassId == cls.Id,
                        include: x => x.Include(x => x.Slot)!);


                        if (cls.Status == ClassStatusEnum.CANCELED.ToString())
                        {
                            await CheckingCanceledClass(_unitOfWork, currentTime, newNotifications, cls);
                            continue;
                        }

                        if (cls.Status == ClassStatusEnum.PROGRESSING.ToString())
                        {
                            await CheckingProgressingClass(currentTime, newNotifications, cls, _unitOfWork);
                        }
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

        private async Task CheckingCanceledClass(IUnitOfWork<MagicLandContext> _unitOfWork, DateTime currentTime, List<Notification> newNotifications, Class cls)
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

                    await GenerateNotification(currentTime, newNotifications, null, NotificationMessageContant.ChangeClassRequestTitle,
                                 NotificationMessageContant.ChangeClassRequestBody(cls.ClassCode!, stu.Student!.FullName!),
                                 NotificationTypeEnum.TransferClass.ToString(), cls.Image!, actionData, _unitOfWork);
                }
            }
        }

        private async Task CheckingUpComingClass(IUnitOfWork<MagicLandContext> _unitOfWork, DateTime currentTime, List<Notification> newNotifications, Class cls)
        {
            var differents = cls.StartDate - currentTime;
            var dayDifferent = differents.Days;

            if (dayDifferent <= 3 && dayDifferent > 0)
            {
                foreach (var stu in cls.StudentClasses)
                {
                    var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                                         {
                                           ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                                           ($"{AttachValueEnum.StudentId}", $"{stu.StudentId}"),
                                         });

                    await GenerateNotification(currentTime, newNotifications, stu.Student!.ParentId, NotificationMessageContant.ClassUpComingTitle,
                           NotificationMessageContant.ClassUpComingBody(stu.Student.FullName!, cls.ClassCode!, dayDifferent),
                           NotificationTypeEnum.UpcomingClass.ToString(), cls.Image!, actionData, _unitOfWork);
                }
            }
        }

        private async Task CheckingProgressingClass(DateTime currentDate, List<Notification> newNotifications, Class cls, IUnitOfWork _unitOfWork)
        {
            var checkingSchedules = cls.Schedules.Where(sc => sc.Date.Date < currentDate.Date && sc.Date.Date > currentDate.Date.AddDays(-10)).ToList();

            foreach (var schedule in checkingSchedules)
            {
                var attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.ScheduleId == schedule.Id, include: x => x.Include(x => x.Student)!);
                var evaluates = await _unitOfWork.GetRepository<Evaluate>().GetListAsync(predicate: x => x.ScheduleId == schedule.Id);

                if (evaluates.Any(evl => evl.Status == null || evl.Status == string.Empty))
                {
                    var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                        {
                          ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                        });

                    await GenerateNotification(currentDate, newNotifications, cls.LecturerId, NotificationMessageContant.MakeUpEvaluateLecturerTitle,
                           NotificationMessageContant.MakeUpEvaluateLecturerBody(cls, schedule.Date, schedule.Slot!.StartTime + " - " + schedule.Slot.EndTime),
                         NotificationTypeEnum.RemindEvaluate.ToString(), cls.Image!, actionData, _unitOfWork);
                }

                if (attendances.All(att => att.IsPresent == null))
                {
                    var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                        {
                          ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                        });

                    await GenerateNotification(currentDate, newNotifications, cls.LecturerId, NotificationMessageContant.MakeUpAttendanceLecturerTitle,
                           NotificationMessageContant.MakeUpAttendanceLecturerBody(cls, schedule.Date, schedule.Slot!.StartTime + " - " + schedule.Slot.EndTime),
                            NotificationTypeEnum.RemindAttendance.ToString(), cls.Image!, actionData, _unitOfWork);

                }
                else if (attendances.Any(att => att.IsPresent == null))
                {
                    foreach (var attendance in attendances)
                    {
                        if (attendance.IsPresent != null)
                        {
                            continue;
                        }

                        var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                        {
                          ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                          ($"{AttachValueEnum.StudentId}", $"{attendance.StudentId}"),
                        });

                        await GenerateNotification(currentDate, newNotifications, null, NotificationMessageContant.MakeUpAttendanceTitle,
                              NotificationMessageContant.MakeUpAttendanceBody(cls.ClassCode!, attendance.Student!.FullName!, schedule.Date),
                              NotificationTypeEnum.RemindAttendance.ToString(), cls.Image!, actionData, _unitOfWork);
                    }
                }
            }

            await HandelSavedStudentClass(currentDate, newNotifications, cls, _unitOfWork);
        }

        private async Task HandelSavedStudentClass(DateTime currentDate, List<Notification> newNotifications, Class cls, IUnitOfWork _unitOfWork)
        {
            var studentSavedClass = cls.StudentClasses.Where(sc => sc.SavedTime != null).ToList();
            if (studentSavedClass != null && studentSavedClass.Any())
            {
                foreach (var savedStudent in studentSavedClass)
                {
                    if (savedStudent.SavedTime!.Value.Date >= currentDate)
                    {
                        var allNewClassOpen = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.CourseId == cls.CourseId && x.Status == ClassStatusEnum.UPCOMING.ToString());
                        if (allNewClassOpen != null && allNewClassOpen.Any())
                        {
                            foreach (var openClass in allNewClassOpen)
                            {
                                var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                                {
                                    ($"{AttachValueEnum.ClassId}", $"{openClass.Id}"),
                                });

                                await GenerateNotification(currentDate, newNotifications, savedStudent.Student!.ParentId, NotificationMessageContant.RemindRegisterNewClassWhenMakeUpTitle,
                                NotificationMessageContant.RemindRegisterNewClassWhenMakeUpBody(savedStudent.Student!.FullName!, openClass.ClassCode!, cls.ClassCode!, openClass.StartDate),
                                NotificationTypeEnum.RemindRegister.ToString(), openClass.Image!, actionData, _unitOfWork);
                            }
                        }
                    }
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
                    var currentTime = BackgoundTime.GetTime();

                    var newNotifications = new List<Notification>();

                    var usersId = await _unitOfWork.GetRepository<User>().GetListAsync(selector: x => x.Id, predicate: x => x.Role!.Name == RoleEnum.PARENT.ToString());
                    foreach (var userId in usersId)
                    {
                        var items = await _unitOfWork.GetRepository<CartItem>().GetListAsync(
                            predicate: x => x.Cart!.UserId == userId && x.ClassId != default);

                        foreach (var item in items)
                        {
                            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == item.ClassId);
                            if (cls == null)
                            {
                                continue;
                            }

                            if (cls.Status != ClassStatusEnum.UPCOMING.ToString())
                            {
                                continue;
                            }

                            if (cls.StartDate.AddDays(-4).Date == currentTime.Date)
                            {
                                string actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                                {
                                  ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                                });

                                await GenerateNotification(currentTime, newNotifications, userId, NotificationMessageContant.LastDayRegisterTitle, NotificationMessageContant.LastDayRegisterBody(cls.ClassCode!),
                                       NotificationTypeEnum.RemindRegister.ToString(), cls.Image!, actionData, _unitOfWork);
                            }

                            if (cls.StartDate.AddDays(-3).Date == currentTime.Date)
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

        private async Task GenerateNotification(DateTime currentTime, List<Notification> newNotifications, Guid? targetUser, string title, string body, string type, string image, string actionData, IUnitOfWork _unitOfWork)
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
                Id = Guid.NewGuid(),
                Title = title,
                Body = body,
                Type = type,
                Image = image,
                CreatedAt = currentTime,
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
                    var currentTime = BackgoundTime.GetTime();

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
                                if (currentTime.Day % 2 == 0)
                                {
                                    string actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                                    {
                                      ($"{AttachValueEnum.CourseId}", $"{course.Id}"),
                                    });

                                    await GenerateNotification(currentTime, newNotifications, userId, NotificationMessageContant.RemindRegisterCourseTitle,
                                    NotificationMessageContant.RemindRegisterCourseBody(course.Name!),
                                   NotificationTypeEnum.RemindRegister.ToString(), course.Image!, actionData, _unitOfWork);
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
