using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    var currentTime = BackgoundTime.GetTime();

                    var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                        predicate: x => x.Status != ClassStatusEnum.CANCELED.ToString() && x.Status != ClassStatusEnum.COMPLETED.ToString(),
                        include: x => x.Include(x => x.Schedules).ThenInclude(sc => sc.Attendances));

                    var newNotifications = new List<Notification>();
                    newNotifications.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Cập Nhập Lúc " + currentTime,
                    });

                    foreach (var cls in classes)
                    {
                        await CheckingDateTime(cls, currentTime, newNotifications, _unitOfWork);
                    }

                    _unitOfWork.GetRepository<Class>().UpdateRange(classes);
                    if (newNotifications.Count > 0)
                    {
                        await _unitOfWork.GetRepository<Notification>().InsertRangeAsync(newNotifications);

                    }
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Updating Classes Got An Error: [{ex.Message}]";
            }
            return "Updating Classes Success";
        }

        private async Task CheckingDateTime(Class cls, DateTime currentTime, List<Notification> newNotifications, IUnitOfWork _unitOfWork)
        {
            var studentClass = (await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.ClassId == cls.Id)).ToList();

            try
            {
                if (cls.StartDate.Date == currentTime.AddDays(3).Date)
                {
                    if (studentClass.Count < cls.LeastNumberStudent)
                    {
                        //await UpdateItem(studentClass, cls, currentTime, ClassStatusEnum.CANCELED, newNotifications, _unitOfWork);
                        return;
                    }
                }

                if (cls.StartDate.Date == currentTime.Date)
                {
                    if (studentClass.Count < cls.LeastNumberStudent)
                    {
                        return;
                    }

                    await UpdateItem(studentClass, cls, currentTime, ClassStatusEnum.PROGRESSING, newNotifications, _unitOfWork);
                    return;
                }

                if (cls.EndDate.Date == currentTime.AddDays(1).Date)
                {
                    await UpdateItem(studentClass, cls, currentTime, ClassStatusEnum.COMPLETED, newNotifications, _unitOfWork);
                    return;
                }


            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task UpdateItem(List<StudentClass> studentClass, Class cls, DateTime currentTime, ClassStatusEnum classStatus, List<Notification> newNotifications, IUnitOfWork _unitOfWork)
        {
            try
            {
                if (studentClass.Count > 0 && studentClass.Any())
                {
                    foreach (var stu in studentClass)
                    {
                        var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == stu.StudentId);

                        var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                        {
                          ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                          ($"{AttachValueEnum.StudentId}", $"{student.Id}"),
                        });

                        await GenerateRemindClassNotification(
                                classStatus == ClassStatusEnum.PROGRESSING
                               ? NotificationMessageContant.ClassStartedTitle
                               : classStatus == ClassStatusEnum.CANCELED
                               ? NotificationMessageContant.ClassCanceledTitle
                               : NotificationMessageContant.ClassCompletedTitle,
                               classStatus == ClassStatusEnum.PROGRESSING
                               ? NotificationMessageContant.ClassStartedBody(student!.FullName!, cls.ClassCode!)
                               : classStatus == ClassStatusEnum.CANCELED
                               ? NotificationMessageContant.ClassCanceledBody(student!.FullName!, cls.ClassCode!)
                               : NotificationMessageContant.ClassCompletedBody(student!.FullName!, cls.ClassCode!),
                               NotificationPriorityEnum.IMPORTANCE.ToString(), cls.Image!, currentTime, actionData, student.ParentId, newNotifications, _unitOfWork);
                    }
                }


                cls.Status = classStatus.ToString();
                UpdateAttendance(cls, classStatus);

                if (classStatus == ClassStatusEnum.PROGRESSING)
                {
                    studentClass.ForEach(sc => sc.CanChangeClass = false);
                    _unitOfWork.GetRepository<StudentClass>().UpdateRange(studentClass);
                    await _unitOfWork.CommitAsync();
                }

            }
            catch (Exception e)
            {
                throw;
            }

        }

        private async Task GenerateRemindClassNotification(string title, string body, string priority, string image, DateTime createAt, string actionData, Guid targetUserId, List<Notification> newNotifications, IUnitOfWork _unitOfWork)
        {
            var listItemIdentify = new List<string>
                {
                          StringHelper.TrimStringAndNoSpace(targetUserId.ToString()),
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
                Priority = priority,
                Image = image,
                CreatedAt = createAt,
                IsRead = false,
                ActionData = actionData,
                UserId = targetUserId,
                Identify = identify,
            });
        }

        private void UpdateAttendance(Class cls, ClassStatusEnum classStatus)
        {
            var schedules = cls.Schedules;

            foreach (var schedule in schedules)
            {
                if (classStatus == ClassStatusEnum.COMPLETED)
                {
                    schedule.Attendances.ToList().ForEach(att => att.IsValid = false);
                }
                if (classStatus == ClassStatusEnum.PROGRESSING)
                {
                    schedule.Attendances.ToList().ForEach(att => att.IsPublic = true);
                }
            }
        }
    }
}
