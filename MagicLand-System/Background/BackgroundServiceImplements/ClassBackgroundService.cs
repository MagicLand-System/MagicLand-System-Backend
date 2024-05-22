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

                        string title = "", body = "", type = "";

                        if (classStatus == ClassStatusEnum.PROGRESSING)
                        {
                            title = NotificationMessageContant.ClassStartedTitle;
                            body = NotificationMessageContant.ClassStartedBody(student!.FullName!, cls.ClassCode!);
                            type = NotificationTypeEnum.ProgressingClass.ToString();
                        }
                        if (classStatus == ClassStatusEnum.CANCELED)
                        {
                            title = NotificationMessageContant.ClassCanceledTitle;
                            body = NotificationMessageContant.ClassCanceledBody(student!.FullName!, cls.ClassCode!);
                            type = NotificationTypeEnum.CanceledClass.ToString();
                        }
                        if (classStatus == ClassStatusEnum.COMPLETED)
                        {
                            title = NotificationMessageContant.ClassCompletedTitle;
                            body = NotificationMessageContant.ClassCompletedBody(student!.FullName!, cls.ClassCode!);
                            type = NotificationTypeEnum.CompletedClass.ToString();
                        }

                        await GenerateRemindClassNotification(title, body, type, cls.Image!, currentTime, actionData, student.ParentId, newNotifications, _unitOfWork);
                    }
                }


                cls.Status = classStatus.ToString();

                if (classStatus == ClassStatusEnum.PROGRESSING)
                {
                    var schedules = cls.Schedules;

                    foreach (var schedule in schedules)
                    {
                        schedule.Attendances.ToList().ForEach(att => att.IsPublic = true);
                    }

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

        private async Task GenerateRemindClassNotification(string title, string body, string type, string image, DateTime createAt, string actionData, Guid targetUserId, List<Notification> newNotifications, IUnitOfWork _unitOfWork)
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
                Type = type,
                Image = image,
                CreatedAt = createAt,
                IsRead = false,
                ActionData = actionData,
                UserId = targetUserId,
                Identify = identify,
            });
        }

        public async Task<string> UpdateAttendanceInTimeAsync()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentTime = BackgoundTime.GetTime();

                    var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.Status == ClassStatusEnum.PROGRESSING.ToString());

                    var updateAttendances = new List<Attendance>();
                    var deleteAttendances = new List<Attendance>();
                    var updateEvaluates = new List<Evaluate>();
                    var deleteEvaluates = new List<Evaluate>();

                    foreach (var cls in classes)
                    {
                        var scheduleHasStudentMakeUp = await _unitOfWork.GetRepository<Attendance>().GetListAsync(
                            predicate: x => x.Schedule!.ClassId == cls.Id && x.MakeUpFromScheduleId != null,
                            include: x => x.Include(x => x.Schedule)!);

                        foreach (var currentAttendance in scheduleHasStudentMakeUp)
                        {
                            if (currentAttendance.Schedule!.Date.Date == currentTime.AddDays(-1).Date)
                            {
                                var currentEvaluate = await _unitOfWork.GetRepository<Evaluate>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == currentAttendance.ScheduleId && x.StudentId == currentAttendance.StudentId);

                                var originAttendance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == currentAttendance.MakeUpFromScheduleId && x.StudentId == currentAttendance.StudentId);
                                var originEvaluate = await _unitOfWork.GetRepository<Evaluate>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == currentAttendance.MakeUpFromScheduleId && x.StudentId == currentAttendance.StudentId);

                                originAttendance.IsPresent = currentAttendance.IsPresent;
                                originAttendance.Note = currentAttendance.Note;
                                originAttendance.IsPublic = true;

                                originEvaluate.Status = currentEvaluate.Status;
                                originEvaluate.Note = currentEvaluate.Note;
                                originEvaluate.IsPublic = true;

                                updateAttendances.Add(originAttendance);
                                updateEvaluates.Add(originEvaluate);
                                deleteAttendances.Add(currentAttendance);
                                deleteEvaluates.Add(currentEvaluate);
                            }
                        }
                    }


                    if (updateAttendances.Count > 0)
                    {
                        _unitOfWork.GetRepository<Attendance>().UpdateRange(updateAttendances);
                    }
                    if (deleteAttendances.Count > 0)
                    {
                        _unitOfWork.GetRepository<Attendance>().DeleteRangeAsync(deleteAttendances);
                    }
                    if (updateEvaluates.Count > 0)
                    {
                        _unitOfWork.GetRepository<Evaluate>().UpdateRange(updateEvaluates);
                    }
                    if (deleteEvaluates.Count > 0)
                    {
                        _unitOfWork.GetRepository<Evaluate>().DeleteRangeAsync(deleteEvaluates);
                    }
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Updating Attendances Got An Error: [{ex.Message}]";
            }
            return "Updating Attendances Success";
        }
    }
}
