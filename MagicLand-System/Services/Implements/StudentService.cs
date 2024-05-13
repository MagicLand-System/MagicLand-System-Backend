using AutoMapper;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request.Attendance;
using MagicLand_System.PayLoad.Request.Evaluates;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Evaluates;
using MagicLand_System.PayLoad.Response.Quizzes.Result;
using MagicLand_System.PayLoad.Response.Quizzes.Result.Student;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Schedules.ForStudent;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;


namespace MagicLand_System.Services.Implements
{
    public class StudentService : BaseService<StudentService>, IStudentService
    {
        public StudentService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<StudentService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        #region thanh_lee code
        public async Task<AccountStudentResponse> AddStudent(CreateStudentRequest studentRequest)
        {
            var currentUser = await ValidateAddNewStudentRequest(studentRequest);
            try
            {
                Guid studentId = Guid.NewGuid();
                var newStudent = _mapper.Map<Student>(studentRequest);
                newStudent.ParentId = currentUser!.Id;
                newStudent.IsActive = true;
                newStudent.Id = studentId;
                var accountsIndex = await GetNextAccountIndex(currentUser);

                var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(predicate: x => x.Name == RoleEnum.STUDENT.ToString(), selector: x => x.Id);
                var newStudentAccount = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = studentRequest.FullName,
                    Phone = currentUser.Phone + "_" + accountsIndex,
                    Email = string.Empty, // studentRequest.Email != null ? studentRequest.Email :
                    Gender = studentRequest.Gender,
                    AvatarImage = studentRequest.AvatarImage,
                    DateOfBirth = studentRequest.DateOfBirth,
                    Address = currentUser.Address,
                    RoleId = role,
                    StudentIdAccount = studentId,
                };

                await _unitOfWork.GetRepository<Student>().InsertAsync(newStudent);
                await _unitOfWork.GetRepository<User>().InsertAsync(newStudentAccount);
                _unitOfWork.Commit();

                var response = _mapper.Map<AccountStudentResponse>(newStudentAccount);
                return response;

            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]" + ex.InnerException != null ? $"[{ex.InnerException}]" : string.Empty,
                          StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<int> GetNextAccountIndex(User currentUser)
        {
            var currentUserAccountStudents = await _unitOfWork.GetRepository<User>().GetListAsync(predicate: x => x.Role!.Name == RoleEnum.STUDENT.ToString());

            if (!currentUserAccountStudents.Any())
            {
                return 1;
            }
            currentUserAccountStudents = currentUserAccountStudents.Where(stu => StringHelper.GetStringWithoutSpecificSyntax(stu.Phone!, "_", true) == currentUser.Phone!).ToList();
            if (!currentUserAccountStudents.Any())
            {
                return 1;
            }

            var accountsIndex = new List<int>();
            foreach (var student in currentUserAccountStudents)
            {
                accountsIndex.Add(int.Parse(StringHelper.GetStringWithoutSpecificSyntax(student.Phone!, "_", false)));
            }
            int maxIndex = accountsIndex.Max();

            return maxIndex + 1;
        }

        private async Task<User> ValidateAddNewStudentRequest(CreateStudentRequest studentRequest)
        {

            int age = DateTime.Now.Year - studentRequest.DateOfBirth.Year;

            if (age < 3 || age > 10)
            {
                throw new BadHttpRequestException("Tuổi Của Bé Phải Từ 3 Đến 10 Tuổi", StatusCodes.Status400BadRequest);
            }
            var students = await GetStudentsOfCurrentParent();
            if (students.Any(stu => stu.FullName!.Trim().ToLower() == studentRequest.FullName.Trim().ToLower()))
            {
                throw new BadHttpRequestException($"Tên [{studentRequest.FullName!}] Của Bé Đã Bị Trùng", StatusCodes.Status400BadRequest);
            }
            //if (students.Any(stu => stu.Email != null && (stu.Email.Trim().ToLower() == studentRequest.Email!.Trim().ToLower())))
            //{
            //    throw new BadHttpRequestException($"Email [{studentRequest.Email!}] Của Bé Đã Bị Trùng", StatusCodes.Status400BadRequest);
            //}

            var currentUser = await GetUserFromJwt();
            if (currentUser == null)
            {
                throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh Không Thể Xác Thực Người Dùng, Vui Lòng Đăng Nhập Lại Và Thực Hiện Lại Thao Tác",
                          StatusCodes.Status500InternalServerError);
            }

            return currentUser;
        }
        public async Task<List<AccountStudentResponse>> GetStudentAccountAsync(Guid? id)
        {
            var students = await GetStudentsOfCurrentParent();
            if (!students.Any())
            {
                return new List<AccountStudentResponse>();
            }
            if (id != null && !students.Any(stu => stu.Id == id))
            {
                throw new BadHttpRequestException($"Id [{id} Của Bé Không Tồn Tại]", StatusCodes.Status400BadRequest);
            }

            var responses = new List<AccountStudentResponse>();
            foreach (var student in students)
            {
                var account = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.StudentIdAccount == student.Id);

                if (id != null && account != null && account.StudentIdAccount == id)
                {
                    responses.Clear();
                    responses.Add(_mapper.Map<AccountStudentResponse>(account));
                    break;
                }
                responses.Add(_mapper.Map<AccountStudentResponse>(account));
            }
            return responses;
        }
        public async Task<List<Student>> GetStudentsOfCurrentParent()
        {
            var students = await _unitOfWork.GetRepository<Student>().GetListAsync(predicate: x => x.ParentId == GetUserIdFromJwt() && x.IsActive == true);
            return students.ToList();
        }

        public async Task<StudentResponse> UpdateStudentAsync(UpdateStudentRequest newStudentInfor, Student oldStudentInfor)
        {
            var studentAccount = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.StudentIdAccount == newStudentInfor.StudentId);
            if (newStudentInfor.DateOfBirth != default)
            {
                int age = DateTime.Now.Year - newStudentInfor.DateOfBirth.Year;
                if (age < 3 || age > 10)
                {
                    throw new BadHttpRequestException("Tuổi Của Học Sinh Phải Bắt Đầu Từ 3 Đến 10 Tuổi", StatusCodes.Status400BadRequest);
                }

                oldStudentInfor.DateOfBirth = newStudentInfor.DateOfBirth;
                studentAccount.DateOfBirth = newStudentInfor.DateOfBirth;
            }
            try
            {
                oldStudentInfor.FullName = newStudentInfor.FullName ?? oldStudentInfor.FullName;
                oldStudentInfor.Gender = newStudentInfor.Gender ?? oldStudentInfor.Gender;
                //oldStudentInfor.Email = newStudentInfor.Email ?? oldStudentInfor.Email;
                oldStudentInfor.AvatarImage = newStudentInfor.AvatarImage ?? oldStudentInfor.AvatarImage;

                studentAccount.FullName = newStudentInfor.FullName ?? oldStudentInfor.FullName;
                studentAccount.Gender = newStudentInfor.Gender ?? oldStudentInfor.Gender;
                //studentAccount.Email = newStudentInfor.Email ?? oldStudentInfor.Email;
                studentAccount.AvatarImage = newStudentInfor.AvatarImage ?? oldStudentInfor.AvatarImage;

                _unitOfWork.GetRepository<Student>().UpdateAsync(oldStudentInfor);
                _unitOfWork.GetRepository<User>().UpdateAsync(studentAccount);
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
            double refundAmount = 0.0;
            string message = "";
            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate: x => x.UserId == GetUserIdFromJwt());

            try
            {
                var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                 predicate: x => x.StudentClasses.Any(sc => sc.StudentId == student.Id));

                var progressingClasses = classes.Where(cls => cls.Status == ClassStatusEnum.PROGRESSING.ToString()).ToList();
                if (progressingClasses.Any())
                {
                    string classCodeString = string.Join(", ", classes.Select(cls => cls.ClassCode).ToList());

                    message = $"Xóa Bé [{student.FullName}] Thành Công, " +
                              $"Hệ Thống Không Hoàn Tiền Lớp [{classCodeString}] Do Lớp Đã Bắt Đầu";

                    refundAmount = await HandelRefundTransaction(progressingClasses, personalWallet, student, true);
                }

                var upcommingClasses = classes.Where(cls => cls.Status == ClassStatusEnum.UPCOMING.ToString()).ToList();
                if (upcommingClasses.Any())
                {
                    string classCodeString = string.Join(", ", classes.Select(cls => cls.ClassCode).ToList());

                    message = $"Xóa Bé [{student.FullName}] Thành Công, " +
                              $"Hệ Thống Đã Hoàn Tiền Lớp [{classCodeString}] Do Lớp Chưa Bắt Đầu";

                    refundAmount = await HandelRefundTransaction(upcommingClasses, personalWallet, student, false);
                }

                await DeleteRelatedStudentInfor(student);

                personalWallet.Balance += refundAmount;
                student.IsActive = false;

                _unitOfWork.GetRepository<Student>().UpdateAsync(student);
                _unitOfWork.GetRepository<PersonalWallet>().UpdateAsync(personalWallet);

                _unitOfWork.Commit();
                return message;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hễ Thống Phát Sinh: [{ex}]");
            }
        }

        private async Task<double> HandelRefundTransaction(List<Class> classes, PersonalWallet personalWallet, Student student, bool isProgressing)
        {
            var id = GetUserIdFromJwt();
            var currentUser = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(id.ToString()), include: x => x.Include(x => x.PersonalWallet!));
            if (currentUser == null)
            {
                throw new Exception($"Lỗi Hễ Thống Phát Sinh Không Thể Xác Thực Người Dùng Vui Lòng Đăng Nhập Lại Và Thực Hiện Lại Giao Dịch");
            }
            var newNotifications = new List<Notification>();
            var refundTransactions = new List<WalletTransaction>();
            double refundAmount = 0.0;

            var oldTransactions = (await _unitOfWork.GetRepository<WalletTransaction>()
           .GetListAsync(predicate: x => x.PersonalWalletId == personalWallet.Id && x.Type == TransactionTypeEnum.Payment.ToString())).ToList();

            foreach (var cls in classes)
            {
                foreach (var trans in oldTransactions)
                {
                    if (!isProgressing)
                    {
                        var result = StringHelper.ExtractAttachValueFromSignature(trans.Signature!);

                        foreach (var pair in result)
                        {
                            if (pair.Key == AttachValueEnum.ClassId.ToString() && pair.Value[0] == cls.Id.ToString())
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
                      ($"{AttachValueEnum.ClassId}", $"{cls.Id}"),
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
            var listItemIdentify = new List<string>
            {
                StringHelper.TrimStringAndNoSpace(targetUser.Id.ToString()),
                StringHelper.TrimStringAndNoSpace(title),
                StringHelper.TrimStringAndNoSpace(body),
                StringHelper.TrimStringAndNoSpace(ImageUrlConstant.RefundImageUrl),
                StringHelper.TrimStringAndNoSpace(actionData),
            };

            string identify = StringHelper.ComputeSHA256Hash(string.Join("", listItemIdentify));

            return new Notification
            {
                Id = new Guid(),
                Title = title,
                Body = body,
                Priority = NotificationPriorityEnum.IMPORTANCE.ToString(),
                Image = ImageUrlConstant.RefundImageUrl,
                CreatedAt = GetCurrentTime(),
                IsRead = false,
                ActionData = actionData,
                UserId = targetUser.Id,
                Identify = identify,
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
                CreateTime = GetCurrentTime(),
                PersonalWalletId = personalWallet.Id,
                PersonalWallet = personalWallet,
                Signature = StringHelper.GenerateTransactionTxnRefCode(TransactionTypeEnum.Refund) + signature.Substring(36),
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

            var studentInClass = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(
                predicate: x => x.StudentId == student.Id && x.Class!.Status != ClassStatusEnum.CANCELED.ToString());

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

            var currentSchedule = schedules.SingleOrDefault(x => x.Date.Date == GetCurrentTime().Date);
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
            var currentSchedule = schedules.SingleOrDefault(x => x.Date.Date == GetCurrentTime().Date);

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
            include: x => x.Include(x => x.Lecture)!
            .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!).Include(x => x.StudentClasses));

            if (studentIdList != null)
            {
                foreach (Guid id in studentIdList)
                {
                    var exsitedStudent = cls.StudentClasses.SingleOrDefault(sc => sc.StudentId == id);
                    if (exsitedStudent == null)
                    {
                        throw new BadHttpRequestException($"Id [{id}] Của Học Sinh Không Tồn Tại Hoặc Không Thuộc Lớp Đang Điểm Danh", StatusCodes.Status400BadRequest);
                    }

                    if (exsitedStudent.SavedTime != null)
                    {
                        throw new BadHttpRequestException($"Id [{id}] Của Học Sinh Thuộc Lớp Này Đã Bảo Lưu Không Thể Điểm Danh", StatusCodes.Status400BadRequest);
                    }
                    continue;
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
                var attendances = await _unitOfWork.GetRepository<Attendance>().GetListAsync(
                    predicate: x => x.ScheduleId == currentSchedule.Id,
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
                    attendance.Note = student.Note;
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
               .GetListAsync(predicate: x => x.AddedTime >= GetCurrentTime().AddDays((int)time), include: x => x.Include(x => x.User));

            return students.Select(stu => _mapper.Map<StudentStatisticResponse>(stu)).ToList();
        }


        public async Task<string> EvaluateStudentAsync(EvaluateRequest request, int noSession)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == request.ClassId,
            include: x => x.Include(x => x.Lecture)!
           .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{request.ClassId} Của Lớp Học Không Tồn Tại]", StatusCodes.Status400BadRequest);
            }

            cls.StudentClasses = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(
            predicate: x => x.ClassId == cls.Id,
            include: x => x.Include(x => x.Student!));

            var result = await CheckingCurrentClassForEvaluate(request.StudeEvaluateRequests, cls, noSession);

            if (result.Any())
            {
                return $"Đánh Giá Hoàn Tất, Một Số Học Sinh [{string.Join(", ", result)}] Không Được Đánh Giá Sẽ Được Hệ Thống Tự Động Đánh Giá";
            }

            return "Đánh Giá Hoàn Tất";
        }

        private async Task<List<string>> EvaluateStudentProgress(List<StudentEvaluateRequest> studentEvaluateRequests, Schedule schedule, List<Student> students)
        {
            try
            {
                var studentNotEvaluate = new List<string>();
                var evaluates = new List<Evaluate>();

                foreach (var student in students)
                {
                    var evaluate = await _unitOfWork.GetRepository<Evaluate>().SingleOrDefaultAsync(
                        predicate: x => x.ScheduleId == schedule.Id && x.StudentId == student.Id);

                    var evaluateStudent = studentEvaluateRequests.SingleOrDefault(se => se.StudentId == student.Id);
                    if (evaluateStudent == null)
                    {
                        evaluate.Status = EvaluateStatusEnum.NORMAL.ToString();
                        studentNotEvaluate.Add(student.FullName!);
                        evaluates.Add(evaluate);

                        continue;
                    }

                    evaluate.Status = evaluateStudent.Level == 1
                    ? EvaluateStatusEnum.NOTGOOD.ToString() : evaluateStudent.Level == 2
                    ? EvaluateStatusEnum.NORMAL.ToString() : EvaluateStatusEnum.GOOD.ToString();
                    evaluate.Note = evaluateStudent.Note!;

                    evaluates.Add(evaluate);
                }

                _unitOfWork.GetRepository<Evaluate>().UpdateRange(evaluates);
                _unitOfWork.Commit();

                return studentNotEvaluate;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hễ Thống Phát Sinh: [{ex.Message}]" + ex.InnerException != null ? $"[{ex.InnerException}]" : "");
            }
        }


        private async Task<List<string>> CheckingCurrentClassForEvaluate(List<StudentEvaluateRequest> studentEvaluateRequests, Class cls, int noSession)
        {

            if (cls.Status!.ToString().Trim() != ClassStatusEnum.PROGRESSING.ToString())
            {

                throw new BadHttpRequestException($"Chỉ Có Thế Điểm Danh Lớp [Đang Diễn Ra] Lớp [{cls.ClassCode}] [{EnumUtil.CompareAndGetDescription<ClassStatusEnum>(cls.Status).Trim()}]", StatusCodes.Status400BadRequest);
            }

            if (cls.Lecture!.Id != GetUserIdFromJwt())
            {
                throw new BadHttpRequestException($"Id [{cls.ClassCode}] Lớp Học Này Không Được Phân Công Dạy Bởi Bạn", StatusCodes.Status400BadRequest);
            }
            if (noSession < 0 || noSession > 30)
            {
                throw new BadHttpRequestException("Thứ Tự Của Buổi Học Không Hợp Lệ", StatusCodes.Status400BadRequest);
            }

            var schedule = cls.Schedules.ToList()[noSession - 1];
            if (schedule.Date.Day > GetCurrentTime().Day)
            {
                throw new BadHttpRequestException($"Số Thứ Tự Buổi Học Thuộc Ngày [{schedule.Date}] Vẫn Chưa Diễn Ra", StatusCodes.Status400BadRequest);
            }

            foreach (Guid id in studentEvaluateRequests.Select(se => se.StudentId).ToList())
            {
                if (cls.StudentClasses.Any(sc => sc.StudentId == id))
                {
                    continue;
                }
                throw new BadHttpRequestException($"Id [{id}] Của Học Sinh Không Tồn Tại Hoặc Không Thuộc Lớp Đang Đánh Giá", StatusCodes.Status400BadRequest);
            }

            return await EvaluateStudentProgress(studentEvaluateRequests, schedule, cls.StudentClasses.Select(sc => sc.Student!).ToList());
        }

        public async Task<List<EvaluateResponse>> GetStudentEvaluatesAsync(Guid classId, int? noSession)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == classId,
            include: x => x.Include(x => x.Lecture)!
           .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId} Của Lớp Học Không Tồn Tại]", StatusCodes.Status400BadRequest);
            }
            if (cls.Lecture!.Id != GetUserIdFromJwt())
            {
                throw new BadHttpRequestException($"Id [{cls.ClassCode}] Lớp Học Này Không Được Phân Công Dạy Bởi Bạn", StatusCodes.Status400BadRequest);
            }

            var schedules = cls.Schedules.ToList();
            var responses = new List<EvaluateResponse>();

            if (noSession != null)
            {
                await GetEvaluates(noSession.Value, schedules[noSession.Value - 1], cls.Id, responses);
            }
            else
            {
                for (int i = 0; i < schedules.Count(); i++)
                {
                    await GetEvaluates(i + 1, schedules[i], cls.Id, responses);
                }
            }

            return responses;

        }

        private async Task GetEvaluates(int noSession, Schedule schedule, Guid classId, List<EvaluateResponse> responses)
        {
            var evaluates = await _unitOfWork.GetRepository<Evaluate>().GetListAsync(
               predicate: x => x.ScheduleId == schedule.Id,
               include: x => x.Include(x => x.Student!).ThenInclude(stu => stu.StudentClasses.Single(sc => sc.ClassId == classId)));

            evaluates = evaluates.Where(eva => !eva.Student!.StudentClasses.Any(sc => sc.SavedTime != null)).ToList();

            responses.Add(EvaluateCustomMapper.fromEvaluateInforRelatedToEvaluateResponse(schedule, evaluates.ToList(), noSession));
        }

        public async Task<List<QuizResultWithStudentWork>> GetStudentQuizFullyInforAsync(Guid classId, List<Guid>? studentIdList, List<Guid>? examIdList, bool isLatestAttempt)
        {
            var currentUserId = GetUserIdFromJwt();

            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.Id == classId && (x.LecturerId == currentUserId || x.Schedules.Select(sc => sc.SubLecturerId).Any(sub => sub == currentUserId)),
                include: x => x.Include(x => x.StudentClasses).ThenInclude(sc => sc.Student)!);

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Lớp Học Này Không Tồn Tại, Hoặc Không Được Phân Công Dạy Bởi Bạn", StatusCodes.Status400BadRequest);
            }

            if (cls.Status != ClassStatusEnum.PROGRESSING.ToString())
            {
                throw new BadHttpRequestException($"Chỉ Có Thể Truy Suất Điểm Của Lớp Đã Bắt Đầu, Lớp Này [{EnumUtil.CompareAndGetDescription<ClassStatusEnum>(cls.Status!)}]",
                    StatusCodes.Status400BadRequest);
            }

            var studentClasses = cls.StudentClasses.ToList();

            if (studentIdList != null && studentIdList.Count > 0)
            {
                var invalidIds = studentIdList.Where(id => !studentClasses.Any(sc => sc.StudentId == id)).ToList();

                if (invalidIds.Any())
                {
                    throw new BadHttpRequestException($"Id Học Sinh [{string.Join(" ,", invalidIds)}] Không Thuộc Lớp Học Đang Truy Suất", StatusCodes.Status400BadRequest);
                }
            }

            var sessionIds = (await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                predicate: x => x.Course!.Id == cls.CourseId,
                selector: x => x.Topics!.SelectMany(tp => tp.Sessions!.Select(ses => ses.Id)))).ToList();

            if (sessionIds == null || sessionIds.Count == 0)
            {
                throw new BadHttpRequestException($"Lớp Học Hiện Đang Thuộc Về Khóa Học Chưa Có Giáo Trình", StatusCodes.Status409Conflict);
            }

            var currentExamIds = new List<Guid>();
            foreach (var sesId in sessionIds)
            {
                var examId = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(
                    predicate: x => x.SessionId == sesId,
                    selector: x => x.Id);

                if (examId != default)
                {
                    currentExamIds.Add(examId);
                }
            }

            if (examIdList != null && examIdList.Count > 0)
            {
                var invalidIds = examIdList.Where(exId => !currentExamIds.Any(ceId => ceId == exId)).ToList();

                if (invalidIds.Any())
                {
                    throw new BadHttpRequestException($"Id Bài Kiểm Tra [{string.Join(" ,", invalidIds)}] Không Thuộc Lớp Học Đang Truy Suất", StatusCodes.Status400BadRequest);
                }
            }

            var studentsLoading = studentIdList?.Any() ?? false ? studentIdList.Select(stuId => studentClasses.First(sc => sc.StudentId == stuId)).ToList() : studentClasses;
            var examsLoading = examIdList?.Any() ?? false ? examIdList : currentExamIds;

            var responses = new List<QuizResultWithStudentWork>();
            foreach (var stu in studentsLoading)
            {
                var response = new QuizResultWithStudentWork
                {
                    StudentId = stu.StudentId,
                    StudentName = stu.Student!.FullName!,
                    ExamInfors = new List<StudentWorkFullyInfor>(),
                };

                foreach (var exlId in examsLoading)
                {
                    var testResults = new List<TestResult>();
                    if (isLatestAttempt)
                    {
                        var testResult = await _unitOfWork.GetRepository<TestResult>().SingleOrDefaultAsync(
                            predicate: x => x.ExamId == exlId && x.StudentClass!.StudentId == stu.StudentId,
                            orderBy: x => x.OrderByDescending(x => x.NoAttempt),
                            include: x => x.Include(x => x.ExamQuestions));

                        if (testResult != null)
                        {
                            testResults.Add(testResult);
                        }
                    }
                    else
                    {
                        var testResult = await _unitOfWork.GetRepository<TestResult>().GetListAsync(
                           predicate: x => x.ExamId == exlId && x.StudentClass!.StudentId == stu.StudentId,
                           orderBy: x => x.OrderBy(x => x.NoAttempt),
                           include: x => x.Include(x => x.ExamQuestions));

                        if (testResult?.Any() ?? false)
                        {
                            testResults.AddRange(testResult);
                        }
                    }

                    if (testResults.Any())
                    {
                        foreach (var test in testResults)
                        {
                            response.ExamInfors.Add(new StudentWorkFullyInfor
                            {
                                ExamId = test.ExamId,
                                ExamName = test.ExamName!,
                                NoAttempt = test.NoAttempt,
                                QuizCategory = test.QuizCategory!,
                                QuizType = test.QuizType!,
                                QuizName = test.QuizName!,
                                TotalMark = test.TotalMark,
                                CorrectMark = test.CorrectMark,
                                TotalScore = test.TotalScore,
                                ScoreEarned = test.ScoreEarned,
                                ExamStatus = test.ExamStatus!,
                                DoingTime = test.DoingTime,
                                StudentWorkResult = test.ExamQuestions?.Any() ?? false ? await GenerateStudentWorkResult(test.ExamQuestions.ToList()) : null,
                            });
                        }
                    }
                }
                responses.Add(response);
            }

            return responses;
        }

        private async Task<List<StudentWorkResult>> GenerateStudentWorkResult(List<ExamQuestion> examQuestions)
        {
            var studentWorkResults = new List<StudentWorkResult>();

            foreach (var examQuestion in examQuestions)
            {
                var mutipleChoiceAnswer = await _unitOfWork.GetRepository<MultipleChoiceAnswer>().SingleOrDefaultAsync(predicate: x => x.ExamQuestionId == examQuestion.Id);
                var flasCardAnswers = await _unitOfWork.GetRepository<FlashCardAnswer>().GetListAsync(predicate: x => x.ExamQuestionId == examQuestion.Id);

                studentWorkResults.Add(new StudentWorkResult
                {
                    QuestionId = examQuestion.QuestionId,
                    QuestionDescription = examQuestion.Question,
                    QuestionImage = examQuestion.QuestionImage,
                    MultipleChoiceAnswerResult = mutipleChoiceAnswer != null ? new MCAnswerResultResponse
                    {
                        StudentAnswerId = mutipleChoiceAnswer.AnswerId == default ? null : mutipleChoiceAnswer.AnswerId,
                        StudentAnswerDescription = mutipleChoiceAnswer.Answer,
                        StudentAnswerImage = mutipleChoiceAnswer.AnswerImage,
                        CorrectAnswerId = mutipleChoiceAnswer.CorrectAnswerId,
                        CorrectAnswerDescription = mutipleChoiceAnswer.CorrectAnswer,
                        CorrectAnswerImage = mutipleChoiceAnswer.CorrectAnswerImage,
                        Score = mutipleChoiceAnswer.Score,
                        Status = mutipleChoiceAnswer.Status,
                    } : null,
                    FlashCardAnswerResult = flasCardAnswers != null && flasCardAnswers.Any() ? flasCardAnswers.Select(fca => new FCAnswerResultResponse
                    {
                        StudentFirstCardAnswerId = fca.LeftCardAnswerId,
                        StudentFirstCardAnswerDecription = fca.LeftCardAnswer,
                        StudentFirstCardAnswerImage = fca.LeftCardAnswerImage,
                        StudentSecondCardAnswerId = fca.RightCardAnswerId,
                        StudentSecondCardAnswerDescription = fca.RightCardAnswer,
                        StudentSecondCardAnswerImage = fca.RightCardAnswerImage,
                        CorrectSecondCardAnswerId = fca.CorrectRightCardAnswerId,
                        CorrectSecondCardAnswerDescription = fca.CorrectRightCardAnswer,
                        CorrectSecondCardAnswerImage = fca.CorrectRightCardAnswerImage,
                        Score = fca.Score,
                        Status = fca.Status,
                    }).ToList() : null,
                });
            }

            return studentWorkResults;
        }
        #endregion
        #region gia_thuong code
        public async Task<List<ClassWithSlotShorten>> GetClassOfStudent(string studentId, string status)
        {
            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(studentId));
            if (student == null)
            {
                throw new BadHttpRequestException("Học sinh không tồn tại", StatusCodes.Status400BadRequest);
            }

            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.StudentClasses.Any(sc => sc.StudentId.ToString().ToLower() == studentId.ToLower()),
                include: x => x.Include(x => x.Lecture).Include(x => x.StudentClasses!));

            if (classes is null)
            {
                return new List<ClassWithSlotShorten>();
            }

            foreach (var cls in classes)
            {
                cls.Course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                predicate: x => x.Id == cls.CourseId);
                // include: x => x.Include(x => x.Syllabus).ThenInclude(cs => cs!.Topics!.OrderBy(cs => cs.OrderNumber))
                //.ThenInclude(tp => tp.Sessions!.OrderBy(tp => tp.NoSession)).ThenInclude(ses => ses.SessionDescriptions!));

                cls.Schedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(
                orderBy: x => x.OrderBy(x => x.Date),
                predicate: x => x.ClassId == cls.Id,
                include: x => x.Include(x => x.Slot!).Include(x => x.Room!));
            }

            if (!string.IsNullOrEmpty(status))
            {
                classes = classes.Where(cls => StringHelper.TrimStringAndNoSpace(cls.Status!) == StringHelper.TrimStringAndNoSpace(status)).ToList();
            }
            var responses = classes.Select(c => _mapper.Map<ClassWithSlotShorten>(c)).ToList();
            if (!responses.Any())
            {
                return new List<ClassWithSlotShorten>();
            }
            foreach (var res in responses)
            {
                res.CoursePrice = await GetDynamicPrice(res.ClassId, true);
            }

            return responses;

        }
        public async Task<List<StudentScheduleResponse>> GetScheduleOfStudent(string studentId)
        {

            var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(studentId));
            if (student == null)
            {
                throw new BadHttpRequestException($"Id [{studentId}] Của Học Sinh Không Tồn Tại", StatusCodes.Status400BadRequest);
            }
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.StudentClasses.Any(sc => sc.StudentId.ToString().Equals(studentId) && sc.SavedTime == null),
                include: x => x.Include(x => x.Schedules.OrderBy(sch => sch.Date)).ThenInclude(sch => sch.Slot)
                .Include(x => x.Schedules.OrderBy(sch => sch.Date)).ThenInclude(sch => sch.Room!));

            if (!classes.Any())
            {
                return new List<StudentScheduleResponse>();
            }
            var listStudentSchedule = new List<StudentScheduleResponse>();
            foreach (var cls in classes)
            {
                var lecturerName = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(selector: x => x.FullName, predicate: x => x.Id.Equals(cls.LecturerId));

                var subject = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                       selector: x => x.SubjectName,
                       predicate: x => x.Id == cls.CourseId);

                var topics = await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                    selector: x => x.Topics,
                    predicate: x => x.Course!.Id == cls.CourseId,
                    include: x => x.Include(x => x.Topics!.OrderBy(tp => tp.OrderNumber)).ThenInclude(tp => tp.Sessions!.OrderBy(ses => ses.NoSession)));

                var identifySession = new List<(int, Guid, Guid)>();

                int sessionIndex = 0;
                foreach (var topic in topics!)
                {
                    foreach (var session in topic.Sessions!)
                    {
                        sessionIndex++;
                        identifySession.Add(new(sessionIndex, topic.Id, session.Id));
                    }
                }

                var schedules = cls.Schedules.ToList();
                for (int i = 0; i < schedules.Count; i++)
                {
                    var schedule = schedules[i];
                    var attendance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == schedule.Id && x.StudentId.ToString().ToLower() == studentId.ToLower());
                    var evaluate = await _unitOfWork.GetRepository<Evaluate>().SingleOrDefaultAsync(predicate: x => x.ScheduleId == schedule.Id && x.StudentId.ToString().ToLower() == studentId.ToLower());

                    var studentSchedule = new StudentScheduleResponse
                    {
                        Address = cls.Street + " " + cls.District + " " + cls.City,
                        ClassName = cls.ClassCode!,
                        ClassId = cls.Id,
                        CourseId = cls.CourseId,
                        ClassSubject = subject!,
                        StudentName = student.FullName!,
                        Date = schedule.Date,
                        ClassCode = cls.ClassCode!,
                        TopicId = identifySession.Find(x => x.Item1 == i + 1).Item2,
                        SessionId = identifySession.Find(x => x.Item1 == i + 1).Item3,
                        DayOfWeek = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                        EndTime = schedule.Slot!.EndTime,
                        StartTime = schedule.Slot.StartTime,
                        LinkURL = schedule.Room!.LinkURL,
                        Method = cls.Method,
                        RoomInFloor = schedule.Room.Floor,
                        RoomName = schedule.Room.Name,
                        AttendanceStatus = attendance != null ? attendance.IsPresent == true ? "Có Mặt" : "Vắng Mặt" : "Chưa Điểm Danh",
                        Note = attendance != null ? attendance.Note : null,
                        LecturerName = lecturerName,
                        EvaluateLevel = evaluate != null ? evaluate.Status == EvaluateStatusEnum.NORMAL.ToString() ? 2 : evaluate.Status == EvaluateStatusEnum.NOTGOOD.ToString() ? 1 : 3 : 0,
                        EvaluateDescription = evaluate != null ? evaluate.Status == EvaluateStatusEnum.NORMAL.ToString() ? "Bình Thường"
                        : evaluate.Status == EvaluateStatusEnum.NOTGOOD.ToString() ? "Không Tốt" : "Tốt" : "",
                        EvaluateNote = evaluate != null ? evaluate.Note : "",
                    };

                    listStudentSchedule.Add(studentSchedule);
                }
            }

            return listStudentSchedule;
        }

        public async Task<List<StudentLearningProgress>> GetStudentLearningProgressAsync(Guid studentId, Guid classId)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                predicate: x => x.StudentClasses.Any(sc => sc.StudentId == studentId) && x.Id == classId,
                include: x => x.Include(x => x.Schedules.OrderBy(x => x.Date)));

            if (cls == null)
            {
                throw new BadHttpRequestException($"Id Của Lớp Học Và Id Của Học Sinh Không Tồn Tại Hoặc Học Sinh Không Thuộc Lớp Đang Truy Suất ", StatusCodes.Status400BadRequest);
            }

            if (cls.Status == ClassStatusEnum.COMPLETED.ToString() || cls.Status == ClassStatusEnum.CANCELED.ToString())
            {
                throw new BadHttpRequestException($"Không Thể Truy Suất Tiến Độ Từ Lớp Học [{EnumUtil.CompareAndGetDescription<ClassStatusEnum>(cls.Status)}] ",
                StatusCodes.Status400BadRequest);
            }

            int totalQuiz = 0, quizDone = 0, learningProgress = 0, attendanceProgress = 0, examProgress = 0;

            var sessions = (await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
                selector: x => x.Topics!.SelectMany(x => x.Sessions!),
                predicate: x => x.Course!.Id == cls.CourseId)).OrderBy(x => x.NoSession).ToList();

            foreach (var session in sessions)
            {
                var quizId = await _unitOfWork.GetRepository<QuestionPackage>().SingleOrDefaultAsync(selector: x => x.Id, predicate: x => x.SessionId == session.Id);
                if (quizId != default)
                {
                    totalQuiz++;
                    var isQuizDone = await _unitOfWork.GetRepository<TestResult>().SingleOrDefaultAsync(predicate: x => x.ExamId == quizId && x.StudentClass!.StudentId == studentId);
                    if (isQuizDone is not null)
                    {
                        quizDone++;
                    }
                }
            }
            examProgress = (quizDone * 100) / totalQuiz;

            var schedules = cls.Schedules.ToList();
            var currentDate = GetCurrentTime().Date;

            for (int i = 0; i < schedules.Count(); i++)
            {
                var scheduleDate = schedules[i].Date.Date;
                var difference = scheduleDate - currentDate;
                int day = difference.Days;

                if (day < 0)
                {
                    continue;
                }

                if (day >= 0)
                {
                    if (i == 0)
                    {
                        break;
                    }

                    learningProgress = (i * 100) / schedules.Count();
                    break;
                }
            }

            int totalAttendance = 0;
            foreach (var schedule in schedules)
            {
                var isAttendance = await _unitOfWork.GetRepository<Attendance>().SingleOrDefaultAsync(
                    selector: x => x.IsPresent,
                    predicate: x => x.ScheduleId == schedule.Id && x.StudentId == studentId);

                if (isAttendance != null && isAttendance.Value)
                {
                    totalAttendance++;
                }
            }
            attendanceProgress = (totalAttendance * 100) / schedules.Count();

            return new List<StudentLearningProgress>
            {
                new StudentLearningProgress{ProgressName = "Attendance", PercentageProgress = attendanceProgress},
                new StudentLearningProgress{ProgressName = "Learning", PercentageProgress = learningProgress},
                new StudentLearningProgress{ProgressName = "Exam", PercentageProgress = examProgress},
            };

        }

        public async Task<List<ScheduleReLearn>> FindValidDayReLearningAsync(Guid studentId, Guid classId, List<DateOnly> dayOffs)
        {
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == classId && x.StudentClasses.Any(sc => sc.StudentId == studentId),
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).Include(x => x.Course)!);

            var student = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(selector: x => x.Students.FirstOrDefault(st => st.Id == studentId), predicate: x => x.Id == GetUserIdFromJwt());
            if (student == null)
            {
                throw new BadHttpRequestException($"Id Học Sinh [{studentId}] Không Hợp Lệ, Khi Không Tồn Tại Hoặc Phụ Huynh Đang Truy Vấn Bé Khác Không Thuộc Tài Khoản Này", StatusCodes.Status400BadRequest);
            }
            if (!student.IsActive!.Value)
            {
                throw new BadHttpRequestException($"Id Học Sinh [{studentId}] Không Hợp Lệ, Khi Không Đã Ngưng Hoạt Động", StatusCodes.Status400BadRequest);
            }
            if (cls == null)
            {
                throw new BadHttpRequestException($"Id [{classId}] Của Lớp Học Không Tồn Tại Hoặc Hoặc Học Sinh Không Thuộc Lớp Đang Truy Suất", StatusCodes.Status400BadRequest);
            }
            if (cls.Status != ClassStatusEnum.PROGRESSING.ToString())
            {
                throw new BadHttpRequestException($"Chỉ Có Thế Điểm Danh Lớp [Đang Diễn Ra] Lớp [{cls.ClassCode}] [{EnumUtil.CompareAndGetDescription<ClassStatusEnum>(cls.Status!).Trim()}]", StatusCodes.Status400BadRequest);
            }

            var duplicates = dayOffs.GroupBy(d => d).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Any())
            {
                throw new BadHttpRequestException($"Ngày Nghĩ Không Hợp Lệ [{string.Join(", ", duplicates.Select(dp => dp.ToString()))}], Khi Đang Bị Trùng Lặp", StatusCodes.Status400BadRequest);
            }

            var schedules = cls.Schedules.ToList();

            foreach (var d in dayOffs)
            {
                if (d <= DateOnly.FromDateTime(GetCurrentTime().Date))
                {
                    throw new BadHttpRequestException($"Ngày Nghĩ Không Hợp Lệ [{d}], Khi Sắp Diễn Ra Hoặc Đã Diễn Ra So Với Ngày Hiện Tại", StatusCodes.Status400BadRequest);
                }

                if (!schedules.Any(sc => d == DateOnly.FromDateTime(sc.Date.Date)))
                {
                    throw new BadHttpRequestException($"Ngày Nghĩ Không Hợp Lệ [{d}], " +
                          $"Khi Không Thuộc Lịch Học Của Lơp Đang Truy Suất", StatusCodes.Status400BadRequest);
                }
            }

            var classRealted = await _unitOfWork.GetRepository<Class>().GetListAsync(
                predicate: x => x.CourseId == cls.CourseId && x.Status == ClassStatusEnum.PROGRESSING.ToString() && x.Id != classId,
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)));

            var responses = new List<ScheduleReLearn>();

            if (classRealted is not null && classRealted.Any())
            {
                var timeSessions = new List<(int, DateTime)>();

                var schedulesOff = schedules.Where(sc => dayOffs.Any(day => day.Equals(DateOnly.FromDateTime(sc.Date.Date)))).ToList();
                foreach (var so in schedulesOff)
                {
                    int noSession = schedules.ToList().IndexOf(so);
                    timeSessions.Add(new(noSession, so.Date));
                }

                timeSessions = timeSessions.OrderBy(ts => ts.Item2).ToList();

                foreach (var timeSession in timeSessions)
                {
                    var response = new ScheduleReLearn
                    {
                        DayOffRequest = DateOnly.FromDateTime(timeSession.Item2.Date),
                    };
                    var scheduleReLearns = new List<ScheduleResponse>();

                    foreach (var cr in classRealted)
                    {
                        var dayOfSession = cr.Schedules.ToList()[timeSession.Item1];

                        if (dayOfSession.Date.Date > timeSession.Item2.Date && !timeSessions.Any(ts => ts.Item2.Date == dayOfSession.Date.Date))
                        {
                            dayOfSession.Slot = await _unitOfWork.GetRepository<Slot>().SingleOrDefaultAsync(predicate: x => x.Id == dayOfSession.SlotId);
                            dayOfSession.Room = await _unitOfWork.GetRepository<Room>().SingleOrDefaultAsync(predicate: x => x.Id == dayOfSession.RoomId);
                            var lecturer = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == cr.LecturerId, include: x => x.Include(x => x.Role)!);

                            var scheduleReLearn = _mapper.Map<ScheduleResponse>(dayOfSession);
                            scheduleReLearn.Lecturer = _mapper.Map<LecturerResponse>(lecturer);
                            scheduleReLearn.ClassCode = cr.ClassCode;
                            scheduleReLearn.ClassName = cr.Course!.Name;
                            scheduleReLearn.ClassSubject = cls.Course!.SubjectName;
                            scheduleReLearn.Method = cls.Method;

                            scheduleReLearns.Add(scheduleReLearn);
                        }
                    }

                    response.Schedules = scheduleReLearns.Any() ? scheduleReLearns : null;
                    responses.Add(response);
                }
            }

            return responses;
        }
    }
    #endregion
}

