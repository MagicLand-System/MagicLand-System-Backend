﻿using AutoMapper;
using Azure.Core;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Diagnostics;
using System.Transactions;


namespace MagicLand_System.Services.Implements
{
    public class StudentService : BaseService<StudentService>, IStudentService
    {
        public StudentService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<StudentService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<bool> AddStudent(CreateStudentRequest request)
        {
            if (request.DateOfBirth > DateTime.Now.AddYears(-3))
            {
                throw new BadHttpRequestException("Hoc sinh phải lớn hơn 3 tuổi", StatusCodes.Status400BadRequest);
            }
            var userId = (await GetUserFromJwt()).Id;
            if (request == null)
            {
                throw new BadHttpRequestException("yêu cầu không hợp lệ", StatusCodes.Status400BadRequest);
            }
            var student = _mapper.Map<Student>(request);
            student.ParentId = userId;
            student.IsActive = true;
            await _unitOfWork.GetRepository<Student>().InsertAsync(student);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            return isSuccess;
        }

        public async Task<List<ClassResExtraInfor>> GetClassOfStudent(string studentId, string status)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(studentId));
            if (student == null)
            {
                throw new BadHttpRequestException("Học sinh không tồn tại", StatusCodes.Status400BadRequest);
            }
            var listClassInstance = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId), include: x => x.Include(x => x.Class));
            if (listClassInstance == null)
            {
                return new List<ClassResExtraInfor>();
            }
            var classIds = (from classInstance in listClassInstance
                            group classInstance by classInstance.ClassId into grouped
                            select new { ClassId = grouped.Key });
            var myx = classIds.ToList();
            Class studentClass = null;
            List<Class> classes = new List<Class>();
            List<Class> allClass = new List<Class>();

            allClass = (await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.Id == x.Id, include: x => x
               .Include(x => x.Lecture!)
               .Include(x => x.StudentClasses)
               .Include(x => x.Course)
               .ThenInclude(c => c.Syllabus)
               .ThenInclude(cs => cs!.Topics.OrderBy(cs => cs.OrderNumber))
               .ThenInclude(tp => tp.Sessions.OrderBy(tp => tp.NoSession))
               .Include(x => x.Schedules.OrderBy(sc => sc.Date))
               .ThenInclude(s => s.Slot)!
               .Include(x => x.Schedules.OrderBy(sc => sc.Date))
               .ThenInclude(s => s.Room)!)).ToList();
            foreach (var classInstance in myx)
            {
                if (status.IsNullOrEmpty())
                {
                    studentClass = allClass.SingleOrDefault(x => x.Id.ToString().Equals(classInstance.ClassId.ToString()));
                }
                else
                {
                    studentClass = allClass.SingleOrDefault(x => x.Id.ToString().Equals(classInstance.ClassId.ToString()) && status.Trim().Equals(x.Status.Trim()));
                }
                if (studentClass != null)
                {
                    classes.Add(studentClass);
                }
            }
            if (classes.Count == 0)
            {
                return new List<ClassResExtraInfor>();
            }
            List<ClassResExtraInfor> responses = new List<ClassResExtraInfor>();
            responses = classes.Select(c => _mapper.Map<ClassResExtraInfor>(c)).ToList();
            return responses;//responses;   
        }

        public async Task<List<StudentScheduleResponse>> GetScheduleOfStudent(string studentId)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(studentId));
            if (student == null)
            {
                throw new BadHttpRequestException("StudentId is not exist", StatusCodes.Status400BadRequest);
            }
            var scheduleIdList = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(student.Id.ToString()), selector: x => x.ScheduleId);
            //var listClassInstance = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId), include: x => x.Include(x => x.Class));
            if (scheduleIdList == null)
            {
                return new List<StudentScheduleResponse>();
            }
            //var sessionIds = new List<Guid>();
            //foreach (var classInstance in listClassInstance)
            //{
            //    sessionIds.Add(classInstance.Class.Id);
            //}
            //if (sessionIds.Count == 0)
            //{
            //    //throw new BadHttpRequestException("Student is in not any schedule", StatusCodes.Status400BadRequest);
            //    return new List<StudentScheduleResponse>();
            //}
            var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.Id == x.Id, include: x => x.Include(x => x.Class).Include(x => x.Slot).Include(x => x.Room).Include(x => x.Class));
            var listStudentSchedule = new List<StudentScheduleResponse>();
            StudentScheduleResponse studentSchedule = null;
            List<Schedule> scheduleList = new List<Schedule>();
            foreach (var Id in scheduleIdList)
            {
                var listResult = (schedules.Where(s => s.Id == Id)).ToList();
                scheduleList.AddRange(listResult);
            }
            foreach (var schedule in scheduleList)
            {
                var lecturerName = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(selector: x => x.FullName, predicate: x => x.Id.Equals(schedule.Class.LecturerId));
                var IsPresent = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(selector: x => x.IsPresent, predicate: x => x.ScheduleId == schedule.Id);
                studentSchedule = new StudentScheduleResponse
                {
                    StudentName = student.FullName,
                    Date = schedule.Date,
                    ClassCode = schedule.Class.ClassCode,
                    DayOfWeek = schedule.DayOfWeek,
                    EndTime = schedule.Slot.EndTime,
                    StartTime = schedule.Slot.StartTime,
                    LinkURL = schedule.Room.LinkURL,
                    Method = schedule.Class.Method,
                    RoomInFloor = schedule.Room.Floor,
                    RoomName = schedule.Room.Name,
                    AttendanceStatus = IsPresent == true ? "Có Mặt" : IsPresent == false ? "Vắng Mặt" : "Chưa Điểm Danh",
                    LecturerName = lecturerName,
                };
                listStudentSchedule.Add(studentSchedule);
            }
            return listStudentSchedule; //listStudentSchedule;
        }

        public async Task<List<Student>> GetStudentsOfCurrentParent()
        {
            var students = await _unitOfWork.GetRepository<Student>().GetListAsync(predicate: x => x.ParentId == GetUserIdFromJwt());
            return students.ToList();
        }

        public async Task<StudentResponse> UpdateStudentAsync(UpdateStudentRequest newStudentInfor, Student oldStudentInfor)
        {
            if (newStudentInfor.DateOfBirth != default)
            {
                int age = DateTime.Now.Year - newStudentInfor.DateOfBirth.Year;
                if (age < 3 || age > 10)
                {
                    throw new BadHttpRequestException("Tuổi Của Học Sinh Phải Bắt Đầu Từ 3 Đến 10 Tuổi", StatusCodes.Status400BadRequest);
                }

                oldStudentInfor.DateOfBirth = newStudentInfor.DateOfBirth;
            }
            try
            {
                oldStudentInfor.FullName = newStudentInfor.FullName ?? oldStudentInfor.FullName;
                oldStudentInfor.Gender = newStudentInfor.Gender ?? oldStudentInfor.Gender;
                oldStudentInfor.Email = newStudentInfor.Email ?? oldStudentInfor.Email;
                oldStudentInfor.AvatarImage = newStudentInfor.AvatarImage ?? oldStudentInfor.AvatarImage;

                _unitOfWork.GetRepository<Student>().UpdateAsync(oldStudentInfor);
                await _unitOfWork.CommitAsync();

                return _mapper.Map<StudentResponse>(oldStudentInfor);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hễ Thống Phát Sinh: [{ex}]");
            }
        }


        public async Task<string> DeleteStudentAsync(Student student)
        {
            var classes = new List<ClassResExtraInfor>();
            double refundAmount = 0.0;
            string message = "";
            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate: x => x.UserId == GetUserIdFromJwt());

            try
            {
                classes = await GetClassOfStudent(student.Id.ToString(), ClassStatusEnum.PROGRESSING.ToString());

                if (classes.Any())
                {
                    string classCodeString = string.Join(", ", classes.Select(cls => cls.ClassCode).ToList());

                    message = $"Xóa Bé [{student.FullName}] Thành Công, " +
                              $"Hệ Thống Không Hoàn Tiền Lớp [{classCodeString}] Do Lớp Đã Bắt Đầu";

                    refundAmount = await HandelRefundTransaction(classes, personalWallet, student, true);
                }

                classes = await GetClassOfStudent(student.Id.ToString(), ClassStatusEnum.UPCOMING.ToString());
                if (classes.Any())
                {
                    string classCodeString = string.Join(", ", classes.Select(cls => cls.ClassCode).ToList());

                    message = $"Xóa Bé [{student.FullName}] Thành Công, " +
                              $"Hệ Thống Đã Hoàn Tiền Lớp [{classCodeString}] Do Lớp Chưa Bắt Đầu";

                    refundAmount = await HandelRefundTransaction(classes, personalWallet, student, false);
                }

                await DeleteRelatedStudentInfor(student);

                personalWallet.Balance += refundAmount;
                student.IsActive = false;

                _unitOfWork.GetRepository<Student>().UpdateAsync(student);
                _unitOfWork.GetRepository<PersonalWallet>().UpdateAsync(personalWallet);

                await _unitOfWork.CommitAsync();
                return message;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hễ Thống Phát Sinh: [{ex}]");
            }
        }

        private async Task<double> HandelRefundTransaction(List<ClassResExtraInfor> classes, PersonalWallet personalWallet, Student student, bool isProgressing)
        {
            var currentUser = await GetUserFromJwt();
            var newNotifications = new List<Notification>();
            var refundTransactions = new List<WalletTransaction>();
            double refundAmount = 0.0;

            var oldTransactions = (await _unitOfWork.GetRepository<WalletTransaction>()
          .GetListAsync(predicate: x => x.PersonalWalletId == personalWallet.Id && x.Status == TransactionTypeEnum.Payment.ToString())).ToList();

            foreach (var cls in classes)
            {
                foreach (var trans in oldTransactions)
                {
                    if (!isProgressing)
                    {
                        var result = StringHelper.ExtractAttachValueFromSignature(trans.Signature!);

                        foreach (var pair in result)
                        {
                            if (pair.Key == AttachValueEnum.ClassId.ToString() && pair.Value[0] == cls.ClassId.ToString())
                            {
                                refundAmount += trans.Money - trans.Discount;
                                refundTransactions.Add(GenerateRefundTransaction(personalWallet, currentUser.FullName!, refundAmount, cls.ClassCode!, trans.Signature!));
                            }
                        }
                    }

                    string title = isProgressing ? NotificationMessageContant.NoRefundTitle : NotificationMessageContant.RefundTitle;
                    string body = isProgressing
                        ? NotificationMessageContant.NoRefundBody(cls.ClassCode!, (trans.Money - trans.Discount).ToString(), student.FullName!)
                        : NotificationMessageContant.RefundBody(cls.ClassCode!, (trans.Money - trans.Discount).ToString(), student.FullName!);

                    string actionData = StringHelper.GenerateJsonString(new List<(string, string)>
                    {
                      ($"{AttachValueEnum.ClassId}", $"{cls.ClassId}"),
                      ($"{AttachValueEnum.StudentId}", $"{student.Id}"),
                      ($"{AttachValueEnum.TransactionId}", $"{trans.Id}"),
                    });

                    newNotifications.Add(GenerateNotification(currentUser, title, body, actionData));
                }
            }

            if (refundTransactions.Any())
            {
                await _unitOfWork.GetRepository<WalletTransaction>().InsertRangeAsync(refundTransactions);
            }
            await _unitOfWork.GetRepository<Notification>().InsertRangeAsync(newNotifications);

            return refundAmount;
        }

        private Notification GenerateNotification(User targetUser, string title, string body, string actionData)
        {
            return new Notification
            {
                Id = new Guid(),
                Title = title,
                Body = body,
                Priority = NotificationPriorityEnum.IMPORTANCE.ToString(),
                Image = ImageUrlConstant.RefundImageUrl,
                CreatedAt = DateTime.Now,
                IsRead = false,
                ActionData = actionData,
                UserId = targetUser.Id,
            };

        }

        private WalletTransaction GenerateRefundTransaction(PersonalWallet personalWallet, string payer, double refundAmount, string className, string signature)
        {
            var transaction = new WalletTransaction
            {
                Id = new Guid(),
                TransactionCode = StringHelper.GenerateTransactionCode(TransactionTypeEnum.Refund),
                Money = refundAmount,
                Type = TransactionTypeEnum.Refund.ToString(),
                Method = TransactionMethodEnum.SystemWallet.ToString(),
                Description = $"Hoàn Tiền Lớp Học {className} Từ Hệ Thống",
                CreateTime = DateTime.Now,
                PersonalWalletId = personalWallet.Id,
                PersonalWallet = personalWallet,
                Signature = StringHelper.GenerateTransactionTxnRefCode(TransactionTypeEnum.Refund) + signature.Substring(11),
                Status = TransactionStatusEnum.Success.ToString(),
                CreateBy = payer,
            };

            return transaction;
        }

        private async Task DeleteRelatedStudentInfor(Student student)
        {
            var studentAttendance = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.StudentId == student.Id);
            if (studentAttendance.Any())
            {
                _unitOfWork.GetRepository<Attendance>().DeleteRangeAsync(studentAttendance);
            }

            var studentInClass = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.StudentId == student.Id);
            if (studentInClass.Any())
            {
                _unitOfWork.GetRepository<StudentClass>().DeleteRangeAsync(studentInClass);
            }

            var studentInCart = await _unitOfWork.GetRepository<StudentInCart>().GetListAsync(predicate: x => x.StudentId == student.Id);

            if (studentInCart.Any())
            {
                _unitOfWork.GetRepository<StudentInCart>().DeleteRangeAsync(studentInCart);
            }
        }

        public async Task<string> TakeStudentAttendanceAsync(AttendanceRequest request, SlotEnum slot)
        {
            if (slot == SlotEnum.Default)
            {
                slot = SlotEnum.Slot1;
            }

            var cls = await CheckingCurrentClass(request.StudentAttendanceRequests.Select(sar => sar.StudentId).ToList(), request.ClassId, slot);

            var schedules = cls.Schedules.Where(sc => sc.Slot!.StartTime.Trim() == EnumUtil.GetDescriptionFromEnum(slot).Trim()).ToList();

            var currentSchedule = schedules.SingleOrDefault(x => x.Date.Date == DateTime.Now.Date);
            var studentNotHaveAttendance = await TakeAttenDanceProgress(request, cls, currentSchedule);

            if (studentNotHaveAttendance.Count() > 0)
            {
                return $"Điểm Danh Hoàn Tất, Một Số Học Sinh [{string.Join(", ", studentNotHaveAttendance)}] Không Được Điểm Danh Sẽ Được Hệ Thống Tự Động Đánh Vắng";
            }

            return "Điểm Danh Hoàn Tất";
        }

        public async Task<List<AttendanceResponse>> GetStudentAttendanceFromClassInNow(Guid classId)
        {
            var cls = await CheckingCurrentClass(null, classId, SlotEnum.Default);

            var schedules = cls.Schedules;
            var currentSchedule = schedules.SingleOrDefault(x => x.Date.Date == DateTime.Now.Date);

            var responses = await GetStudentAttendanceProgress(cls, currentSchedule);

            return responses;
        }

        private async Task<List<AttendanceResponse>> GetStudentAttendanceProgress(Class cls, Schedule? currentSchedule)
        {
            if (currentSchedule == null)
            {
                throw new BadHttpRequestException($"Lớp Học [{cls.ClassCode}] Hôm Nay Không Có Lịch Để Điểm Danh", StatusCodes.Status400BadRequest);
            }

            var attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.ScheduleId == currentSchedule.Id,
                include: x => x.Include(x => x.Student)!.Include(x => x.Schedule)!);

            var responses = new List<AttendanceResponse>();

            foreach (var attendance in attendances)
            {
                responses.Add(_mapper.Map<AttendanceResponse>(attendance));
            }

            return responses;
        }

        private async Task<Class> CheckingCurrentClass(List<Guid>? studentIdList, Guid classId, SlotEnum slot)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == classId,
            include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).Include(x => x.Lecture)!
            .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!).Include(x => x.StudentClasses));

            if (studentIdList != null)
            {
                foreach (Guid id in studentIdList)
                {
                    if (cls.StudentClasses.Any(sc => sc.StudentId == id))
                    {
                        continue;
                    }
                    throw new BadHttpRequestException($"Id [{id}] Của Học Sinh Không Tồn Tại Hoặc Không Thuộc Lớp Đang Điểm Danh", StatusCodes.Status400BadRequest);
                }
            }

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại Hoặc Lớp Học Không Có Lịch Học", StatusCodes.Status400BadRequest);
            }

            if (cls.Status!.ToString().Trim() != ClassStatusEnum.PROGRESSING.ToString())
            {
                string statusError = cls.Status!.ToString().Trim() == ClassStatusEnum.UPCOMING.ToString() ? "Sắp Diễn Ra" : "Đã Hoàn Thành";

                throw new BadHttpRequestException($"Chỉ Có Thế Điểm Danh Lớp [Đang Diễn Ra] Lớp [{cls.ClassCode}] [{statusError}]", StatusCodes.Status400BadRequest);
            }

            if (slot != SlotEnum.Default && (!cls.Schedules.Any(sc => sc.Slot!.StartTime.Trim() == EnumUtil.GetDescriptionFromEnum(slot).Trim())))
            {
                throw new BadHttpRequestException($"Lớp Học Không Có Lịch Điểm Danh Slot [{slot}] ", StatusCodes.Status400BadRequest);
            }

            if (cls.Lecture!.Id != GetUserIdFromJwt())
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Này Không Được Phân Công Dạy Bởi Bạn", StatusCodes.Status400BadRequest);
            }

            return cls;
        }

        private async Task<List<string>> TakeAttenDanceProgress(AttendanceRequest request, Class cls, Schedule? currentSchedule)
        {
            if (currentSchedule == null)
            {
                throw new BadHttpRequestException($"Lớp Học [{cls.ClassCode}] Hôm Nay Không Có Lịch Để Điểm Danh", StatusCodes.Status400BadRequest);
            }

            var studentNotHaveAttendance = new List<string>();

            try
            {
                var attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.ScheduleId == currentSchedule.Id,
                   include: x => x.Include(x => x.Student)!);
                var studentAttendanceRequest = request.StudentAttendanceRequests;

                foreach (var attendance in attendances)
                {
                    var student = studentAttendanceRequest.SingleOrDefault(stu => stu.StudentId == attendance.StudentId);

                    if (student == null)
                    {
                        studentNotHaveAttendance.Add(attendance.Student!.FullName!);
                        attendance.IsPresent = false;
                        continue;
                    }
                    attendance.IsPresent = student.IsPresent;
                }

                _unitOfWork.GetRepository<Attendance>().UpdateRange(attendances);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hễ Thống Phát Sinh: [{ex}]");
            }

            return studentNotHaveAttendance;
        }

        public async Task<StudentResponse> GetStudentById(Guid id)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == id);
            if (student == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Học Sinh Không Tồn Tại", StatusCodes.Status400BadRequest);
            }

            return _mapper.Map<StudentResponse>(student);
        }

        public async Task<List<StudentStatisticResponse>> GetStatisticNewStudentRegisterAsync(PeriodTimeEnum time)
        {
            if (time == PeriodTimeEnum.Default)
            {
                time = PeriodTimeEnum.Week;
            }

            var students = await _unitOfWork.GetRepository<Student>()
               .GetListAsync(predicate: x => x.AddedTime >= DateTime.Now.AddDays((int)time), include: x => x.Include(x => x.User));

            return students.Select(stu => _mapper.Map<StudentStatisticResponse>(stu)).ToList();
        }
    }
}
