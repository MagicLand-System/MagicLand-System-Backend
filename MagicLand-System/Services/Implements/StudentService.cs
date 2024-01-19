using AutoMapper;
using Azure.Core;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
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
                throw new BadHttpRequestException("student must be at least 3 yearolds", StatusCodes.Status400BadRequest);
            }
            var userId = (await GetUserFromJwt()).Id;
            if (request == null)
            {
                throw new BadHttpRequestException("request is invalid", StatusCodes.Status400BadRequest);
            }
            var student = _mapper.Map<Student>(request);
            student.ParentId = userId;
            await _unitOfWork.GetRepository<Student>().InsertAsync(student);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            return isSuccess;
        }

        public async Task<List<ClassResExtraInfor>> GetClassOfStudent(string studentId, string status)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(studentId));
            if (student == null)
            {
                throw new BadHttpRequestException("StudentId is not exist", StatusCodes.Status400BadRequest);
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
               .ThenInclude(c => c.CourseSyllabus)
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
            var listClassInstance = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate: x => x.StudentId.ToString().Equals(studentId), include: x => x.Include(x => x.Class));
            if (listClassInstance == null)
            {
                throw new BadHttpRequestException("Student is in not any class", StatusCodes.Status400BadRequest);
            }
            var sessionIds = new List<Guid>();
            foreach (var classInstance in listClassInstance)
            {
                sessionIds.Add(classInstance.Class.Id);
            }
            if (sessionIds.Count == 0)
            {
                //throw new BadHttpRequestException("Student is in not any schedule", StatusCodes.Status400BadRequest);
                return new List<StudentScheduleResponse>();
            }
            var schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.Id == x.Id, include: x => x.Include(x => x.Class).Include(x => x.Slot).Include(x => x.Room).Include(x => x.Class));
            var listStudentSchedule = new List<StudentScheduleResponse>();
            StudentScheduleResponse studentSchedule = null;
            List<Schedule> scheduleList = new List<Schedule>();
            foreach (var Id in sessionIds)
            {
                var listResult = (schedules.Where(s => s.ClassId == Id)).ToList();
                scheduleList.AddRange(listResult);
            }
            foreach (var schedule in scheduleList)
            {
                var lecturerName = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(selector: x => x.FullName, predicate: x => x.Id.Equals(schedule.Class.LecturerId));
                studentSchedule = new StudentScheduleResponse
                {
                    StudentName = student.FullName,
                    Date = schedule.Date,
                    DayOfWeek = schedule.DayOfWeek,
                    EndTime = schedule.Slot.EndTime,
                    StartTime = schedule.Slot.StartTime,
                    LinkURL = schedule.Room.LinkURL,
                    Method = schedule.Class.Method,
                    RoomInFloor = schedule.Room.Floor,
                    RoomName = schedule.Room.Name,
                    ClassName = schedule.Class.Name,
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
                    message = $"Xóa Bé [{student.FullName}] Thành Công, " +
                              $"Hệ Thống Không Hoàn Tiền Lớp [{string.Join(", ", classes.Select(cls => cls.Name).ToList())}] Do Lớp Đã Bắt Đầu";
                }

                classes = await GetClassOfStudent(student.Id.ToString(), ClassStatusEnum.UPCOMING.ToString());
                if (classes.Any())
                {
                    message = $"Xóa Bé [{student.FullName}] Thành Công, " +
                              $"Hệ Thống Đã Hoàn Tiền Lớp [{string.Join(", ", classes.Select(cls => cls.Name).ToList())}] Do Lớp Chưa Bắt Đầu";

                    refundAmount = await AddRefundTransaction(classes, personalWallet);
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

        private async Task<double> AddRefundTransaction(List<ClassResExtraInfor> classes, PersonalWallet personalWallet)
        {
            var oldTransactions = (await _unitOfWork.GetRepository<WalletTransaction>().GetListAsync(predicate: x => x.PersonalWalletId == personalWallet.Id)).ToList();
            var refundTransactions = new List<WalletTransaction>();
            double refundAmount = 0.0;

            foreach (var cls in classes)
            {
                foreach (var trans in oldTransactions)
                {
                    string description = trans.SystemDescription!;
                    var classCodes = StringHelper.ExtractValuesFromTransactionDescription(description, TransactionDescriptionEnum.ClassCodes.ToString(), true);

                    if (classCodes.Contains(cls.ClassCode!))
                    {
                        refundAmount += trans.Money;

                        var transaction = new WalletTransaction
                        {
                            Id = new Guid(),
                            Money = trans.Money,
                            Type = TransactionTypeEnum.Refund.ToString(),
                            SystemDescription = trans.SystemDescription,
                            CreatedTime = DateTime.Now,
                            PersonalWalletId = personalWallet.Id,
                            PersonalWallet = personalWallet,
                            IsProcessed = true,
                        };

                        refundTransactions.Add(transaction);
                    }
                }
            }

            await _unitOfWork.GetRepository<WalletTransaction>().InsertRangeAsync(refundTransactions);

            return refundAmount;
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

        public async Task<string> TakeStudentAttendanceAsync(AttendanceRequest request)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == request.ClassId,
                include: x => x.Include(x => x.Schedules).Include(x => x.Lecture)!);

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{request.ClassId}] Của Lớp Học Không Tồn Tại Hoặc Lớp Học Không Có Lịch Học", StatusCodes.Status400BadRequest);
            }

            if(cls.Lecture!.Id != GetUserIdFromJwt())
            {
                throw new BadHttpRequestException($"Id [{request.ClassId}] Lớp Học Này Không Được Phân Công Dạy Bởi Bạn", StatusCodes.Status400BadRequest);
            }

            var schedules = cls.Schedules;
            var currentSchedule = schedules.SingleOrDefault(x => x.Date == DateTime.Now);

            if (currentSchedule == null)
            {
                throw new BadHttpRequestException($"Lớp Học [{cls.Name}] Hôm Nay Không Có Lịch Để Điểm Danh", StatusCodes.Status400BadRequest);
            }

            var attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(predicate: x => x.ScheduleId == currentSchedule.Id, include: x => x.Include(x => x.Student)!);
            var studentAttendanceRequest = request.StudentAttendanceRequests;
            var studentNotHaveAttendance = new List<string>();

            foreach (var attendance in attendances)
            {
                foreach (var stuAttReq in studentAttendanceRequest)
                {
                    if (stuAttReq.StudentId == attendance.StudentId)
                    {
                        attendance.IsPresent = stuAttReq.IsAttendance;
                        studentAttendanceRequest.Remove(stuAttReq);
                        continue;
                    }

                    studentNotHaveAttendance.Add(attendance.Student!.FullName!);
                    attendance.IsPresent = false;
                }
            }

            if(studentNotHaveAttendance.Count() > 0)
            {
                return $"Điểm Danh Hoàn Tất, Một Số Học Sinh [{string.Join(", ", studentNotHaveAttendance)}] Không Được Điểm Danh Sẽ Được Hệ Thống Tự Động Đánh Vắn";
            }

            return "Điểm Danh Hoàn Tất";
        }
    }
}
