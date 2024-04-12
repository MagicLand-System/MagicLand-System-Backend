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
                        include: x => x.Include(x => x.StudentClasses).ThenInclude(sc => sc.Student).Include(x => x.Schedules).ThenInclude(sc => sc.Attendances));

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

                    await _unitOfWork.GetRepository<Notification>().InsertRangeAsync(newNotifications);
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
            try
            {
                //if(cls.ClassCode.ToLower() == "LTS101-1".ToLower())
                //{
                //    var a = "g";
                //}

                if (cls.StartDate.Date == currentTime.AddDays(3).Date)
                {
                    cls.Status = ClassStatusEnum.LOCKED.ToString();

                    if (cls.StudentClasses.Any() && cls.StudentClasses != null)
                    {
                        cls.StudentClasses.ToList().ForEach(stu => stu.CanChangeClass = false);
                    }

                    _unitOfWork.GetRepository<Class>().UpdateAsync(cls);
                    await _unitOfWork.CommitAsync();
                    return;
                }

                if (cls.StartDate.Date == currentTime.Date)
                {
                    //if (cls.StudentClasses.Count() < cls.LeastNumberStudent)
                    //{
                    //    UpdateAttendance(cls, ClassStatusEnum.CANCELED.ToString());
                    //    return;
                    //}

                    await UpdateItem(cls, currentTime, ClassStatusEnum.PROGRESSING, newNotifications, _unitOfWork);
                    _unitOfWork.GetRepository<Class>().UpdateAsync(cls);
                    await _unitOfWork.CommitAsync();
                    return;
                }

                if (cls.EndDate.Date == currentTime.AddDays(-1).Date)
                {
                    await UpdateItem(cls, currentTime, ClassStatusEnum.COMPLETED, newNotifications, _unitOfWork);
                    _unitOfWork.GetRepository<Class>().UpdateAsync(cls);
                    await _unitOfWork.CommitAsync();
                    return;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task UpdateItem(Class cls, DateTime currentTime, ClassStatusEnum classStatus, List<Notification> newNotifications, IUnitOfWork _unitOfWork)
        {
            var studentClass = cls.StudentClasses.ToList();
            if (studentClass.Count > 0 && !studentClass.Any())
            {
                foreach (var stu in studentClass)
                {
                    var actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                        {
                          ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
                          ($"{AttachValueEnum.StudentId}", $"{stu.StudentId}"),
                        });

                    await GenerateRemindClassNotification(classStatus == ClassStatusEnum.PROGRESSING
                           ? NotificationMessageContant.ClassStartedTitle
                           : NotificationMessageContant.ClassCompletedTitle,
                           classStatus == ClassStatusEnum.PROGRESSING
                           ? NotificationMessageContant.ClassStartedBody(stu.Student!.FullName!, cls.ClassCode!)
                           : NotificationMessageContant.ClassCompletedBody(stu.Student!.FullName!, cls.ClassCode!),
                           NotificationPriorityEnum.IMPORTANCE.ToString(), cls.Image!, currentTime, actionData, stu.Student.ParentId, newNotifications, _unitOfWork);

                    stu.CanChangeClass = false;
                }
            }

            cls.Status = classStatus.ToString();
            UpdateAttendance(cls, classStatus);
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
