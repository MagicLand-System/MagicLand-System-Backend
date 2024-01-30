using AutoMapper;
using MagicLand_System.Constants;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Request.Cart;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response.Bills;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.WalletTransactions;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MagicLand_System.Services.Implements
{
    public class WalletTransactionService : BaseService<WalletTransactionService>, IWalletTransactionService
    {
        public WalletTransactionService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<WalletTransactionService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }


        public async Task<WalletTransactionResponse> GetWalletTransaction(string id)
        {
            var transactions = await GetWalletTransactions();
            if (transactions == null || transactions.Count == 0)
            {
                return new WalletTransactionResponse();
            }
            return transactions.SingleOrDefault(x => x.TransactionId.ToString().ToLower().Equals(id.ToLower()));
        }

        public async Task<List<WalletTransactionResponse>> GetWalletTransactions(string phone = null, DateTime? startDate = null, DateTime? endDate = null)
        {

            var transactions = await _unitOfWork.GetRepository<WalletTransaction>().GetListAsync( predicate: x => x.Type != TransactionTypeEnum.TopUp.ToString(),
                include: x => x.Include(x => x.PersonalWallet).ThenInclude(x => x.User).ThenInclude(x => x.Students));
            if (transactions == null || transactions.Count == 0)
            {
                return new List<WalletTransactionResponse>();
            }
            List<WalletTransactionResponse> result = new List<WalletTransactionResponse>();
            foreach (var transaction in transactions)
            {
                var rs = StringHelper.ExtractAttachValueFromSignature(transaction.Signature!);

                Guid classId = default;
                List<Guid> studentIdList = new List<Guid>();

                foreach (var pair in rs)
                {
                    if (pair.Key == TransactionAttachValueEnum.ClassId.ToString())
                    {
                        classId = Guid.Parse(pair.Value[0]);
                        continue;
                    }
                    if (pair.Key == TransactionAttachValueEnum.StudentId.ToString())
                    {
                        studentIdList = pair.Value.Select(v => Guid.Parse(v)).ToList();
                        continue;
                    }
                }
                List<Student> students = new List<Student>();
                foreach (var student in studentIdList)
                {
                    var studentx = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(student.ToString()));
                    students.Add(studentx);
                }
                var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(transaction.PersonalWalletId.ToString()));
                var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(personalWallet.UserId.ToString()));
                var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId));
                var courseId = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(classId), selector: x => x.CourseId);
                var courseName = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate: x => x.Id.ToString().Equals(courseId.ToString()), selector: x => x.Name);
                WalletTransactionResponse response = new WalletTransactionResponse
                {
                    CourseName = courseName,
                    CreatedTime = transaction.CreateTime,
                    Description = transaction.Description,
                    Method = transaction.Method,
                    Money = transaction.Money,
                    Parent = new PayLoad.Response.Users.UserResponse
                    {
                        Address = user.Address,
                        AvatarImage = user.AvatarImage,
                        DateOfBirth = user.DateOfBirth,
                        Email = user.Email,
                        FullName = user.FullName,
                        Gender = user.Gender,
                        Id = user.Id,
                        Phone = user.Phone
                    },
                    MyClassResponse = classx,
                    TransactionCode = transaction.TransactionCode,
                    Type = transaction.Type,
                    TransactionId = transaction.Id,
                    Students = students,
                };

                result.Add(response);
            }
            if (endDate != null) { endDate = endDate.Value.AddHours(23).AddMinutes(59); }
            result = (result.OrderByDescending(x => x.CreatedTime)).ToList();
            if (phone == null && startDate == null && endDate == null)
            {
                return (result.OrderByDescending(x => x.CreatedTime)).ToList();
            }
            if (phone != null && startDate == null && endDate == null)
            {
                return (result.Where(x => x.Parent.Phone.ToLower().Equals(phone.ToLower()))).ToList();
            }
            if (phone == null && startDate != null && endDate == null)
            {
                return result = (result.Where(x => x.CreatedTime >= startDate)).ToList();
            }
            if (phone != null && startDate != null && endDate == null)
            {
                return (result.Where(x => (x.CreatedTime >= startDate && x.Parent.Phone.ToLower().Equals(phone.ToLower())))).ToList();
            }
            if (phone == null && startDate != null && endDate != null)
            {
                return (result.Where(x => (x.CreatedTime >= startDate && x.CreatedTime <= endDate))).ToList();
            }
            if (phone == null && startDate == null && endDate != null)
            {
                return (result.Where(x => (x.CreatedTime <= endDate))).ToList();
            }
            if (phone != null && startDate == null && endDate != null)
            {
                return (result.Where(x => (x.CreatedTime <= endDate && x.Parent.Phone.Equals(phone)))).ToList();
            }
            if (endDate < startDate)
            {
                return new List<WalletTransactionResponse>();
            }
            return (result.Where(x => (x.Parent.Phone.ToLower().Equals(phone.ToLower()) && x.CreatedTime >= startDate && x.CreatedTime <= endDate))).ToList();
        }

        public async Task<BillPaymentResponse> CheckoutAsync(List<CheckoutRequest> requests)
        {
            var currentPayer = await GetUserFromJwt();
            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate: x => x.UserId.Equals(GetUserIdFromJwt()));

            double total = await CalculateTotal(requests);

            if (currentPayer.PersonalWallet!.Balance < total)
            {
                throw new BadHttpRequestException($"Số Dư Không Đủ, Yều Cầu Tài Khoản Có Ít Nhất: [{total} d]", StatusCodes.Status400BadRequest);
            }

            double discount = CalculateDiscountEachItem(requests.Count(), total);

            var messageList = await PurchaseProgress(requests, personalWallet, currentPayer, discount);

            return RenderBill(currentPayer, messageList, total, discount * requests.Count());
        }

        private async Task<List<string>> PurchaseProgress(List<CheckoutRequest> requests, PersonalWallet personalWallet, User currentPayer, double discountEachItem)
        {
            var messageList = new List<string>();

            foreach (var request in requests)
            {
                var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                    predicate: x => x.Id == request.ClassId,
                    include: x => x.Include(x => x.Course!)
                    .Include(x => x.Schedules)
                    .Include(x => x.StudentClasses));

                var currentRequestTotal = cls.Course!.Price * request.StudentIdList.Count();

                string studentNameString = await GenerateStudentNameString(request.StudentIdList);

                var newStudentAttendanceList = await RenderStudentAttendanceList(cls.Id, request.StudentIdList);

                var newTransaction = new WalletTransaction
                {
                    Id = new Guid(),
                    TransactionCode = StringHelper.GenerateTransactionCode(TransactionTypeEnum.Payment),
                    Money = currentRequestTotal,
                    Discount = discountEachItem,
                    Type = TransactionTypeEnum.Payment.ToString(),
                    Method = TransactionMethodEnum.SystemWallet.ToString(),
                    Description = $"Đăng Ký Học Sinh {studentNameString} Vào Lớp {cls.Name}",
                    CreateTime = DateTime.Now,
                    PersonalWalletId = personalWallet.Id,
                    PersonalWallet = personalWallet,
                    CreateBy = currentPayer.FullName,
                    Signature = StringHelper.GenerateTransactionTxnRefCode(TransactionTypeEnum.Payment) + StringHelper.GenerateAttachValueForTxnRefCode(new ItemGenerate
                    {
                        ClassId = request.ClassId,
                        StudentIdList = request.StudentIdList
                    }),
                    Status = TransactionStatusEnum.Success.ToString(),
                };

                var newStudentInClassList = request.StudentIdList.Select(sil =>
                new StudentClass
                {
                    Id = new Guid(),
                    StudentId = sil,
                    ClassId = cls.Id,
                }).ToList();

                personalWallet.Balance = personalWallet.Balance - currentRequestTotal;

                await SavePurchaseProgressed(cls, personalWallet, newTransaction, newStudentInClassList, newStudentAttendanceList);

                string message = "Học Sinh [" + studentNameString + $"] Đã Được Thêm Vào Lớp [{cls.Name}]";
                messageList.Add(message);
            }

            return messageList;
        }

        private async Task SavePurchaseProgressed(
            Class cls,
            PersonalWallet personalWallet,
            WalletTransaction newTransaction,
            List<StudentClass> newStudentInClassList,
            List<Attendance> newStudentAttendanceList)
        {
            try
            {
                if (cls.StudentClasses.Count() + newStudentInClassList.Count() >= cls.LeastNumberStudent)
                {
                    await UpdateStudentAttendance(cls);
                    newStudentAttendanceList.ForEach(attendance => attendance.IsPublic = true);
                }

                _unitOfWork.GetRepository<PersonalWallet>().UpdateAsync(personalWallet);
                await _unitOfWork.GetRepository<StudentClass>().InsertRangeAsync(newStudentInClassList);
                await _unitOfWork.GetRepository<Attendance>().InsertRangeAsync(newStudentAttendanceList);
                await _unitOfWork.GetRepository<WalletTransaction>().InsertAsync(newTransaction);

                await _unitOfWork.CommitAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hệ Thống Phát Sinh Khi Sử Lý Thanh Toán Lớp [{cls.Name}]" + ex.InnerException!.ToString());
            }
        }

        private async Task UpdateStudentAttendance(Class cls)
        {
            try
            {
                foreach (var schedule in cls.Schedules)
                {
                    var presentAttendances = await _unitOfWork.GetRepository<Attendance>()
                    .GetListAsync(predicate: x => x.ScheduleId == schedule.Id);

                    if (presentAttendances.Any())
                    {
                        presentAttendances.ToList().ForEach(attendance => attendance.IsPublic = true);
                        _unitOfWork.GetRepository<Attendance>().UpdateRange(presentAttendances);

                        await _unitOfWork.CommitAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hệ Thống Phát Sinh Khi Sử Lý Thanh Toán Lớp [{cls.Name}]" + ex.InnerException!.ToString());
            }
        }

        private BillPaymentResponse RenderBill(User currentPayer, List<string> messageList, double total, double discount)
        {
            var bill = new BillPaymentResponse
            {
                Status = TransactionStatusMessageConstant.Success,
                Message = string.Join(" , ", messageList),
                MoneyAmount = total,
                Discount = discount,
                MoneyPaid = total - discount,
                Date = DateTime.Now,
                Method = TransactionMethodEnum.SystemWallet.ToString(),
                Type = TransactionTypeEnum.Payment.ToString(),
                Payer = currentPayer.FullName!,
            };

            return bill;
        }

        private async Task<List<Attendance>> RenderStudentAttendanceList(Guid classId, List<Guid> studentIds)
        {
            var studentAttendanceList = new List<Attendance>();

            var classSchedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.Class!.Id == classId);

            foreach (var schedule in classSchedules)
            {
                var studentAttendance = studentIds.Select(si => new Attendance
                {
                    Id = new Guid(),
                    StudentId = si,
                    ScheduleId = schedule.Id,
                    IsPresent = null,
                    IsPublic = false,
                }).ToList();

                studentAttendanceList.AddRange(studentAttendance);
            }

            return studentAttendanceList;
        }

        public async Task<bool> ValidRegisterAsync(List<StudentScheduleResponse> allStudentSchedules, Guid classId, List<Guid> studentIds)
        {
            var cls = await _unitOfWork.GetRepository<Class>()
                .SingleOrDefaultAsync(predicate: x => x.Id.Equals(classId), include: x => x
                .Include(x => x.Schedules)
                .ThenInclude(s => s.Slot)
                .Include(x => x.Schedules)
                .ThenInclude(s => s.Room)!
                .Include(x => x.StudentClasses)
                .Include(x => x.Course)!);

            await ValidateSuitableClass(studentIds, cls);

            ValidateSchedule(allStudentSchedules, cls);

            return true;
        }

        private async Task ValidateSuitableClass(List<Guid> studentIds, Class cls)
        {
            string status = cls.Status!.Trim().Equals(ClassStatusEnum.COMPLETED.ToString()) ? "Đã Hoàn Thành" : "Đã Bắt Đầu";

            if (!cls.Status!.Trim().Equals("UPCOMING"))
            {
                throw new BadHttpRequestException($"Xin Lỗi Bạn Chỉ Có Thể Đăng Ký Lớp [Sắp Bắt Đầu], Lớp Này [{status}]",
                    StatusCodes.Status400BadRequest);
            }

            foreach (Guid id in studentIds)
            {
                var student = await _unitOfWork.GetRepository<Student>()
                .SingleOrDefaultAsync(predicate: x => x.Id.Equals(id));

                if (cls.StudentClasses.Any(sc => sc.StudentId.Equals(id)))
                {
                    throw new BadHttpRequestException($"Học Sinh [{student.FullName}] Đã Có Trong Lớp [{cls.Name}]", StatusCodes.Status400BadRequest);
                }

                int age = DateTime.Now.Year - student.DateOfBirth.Year;

                if (age > cls.Course!.MaxYearOldsStudent || age < cls.Course.MinYearOldsStudent)
                {
                    throw new BadHttpRequestException($"Học Sinh [{student.FullName}] Có Độ Tuổi Không Phù Hợp Với Lớp [{cls.Name}]", StatusCodes.Status400BadRequest);
                }

                await ValidateCoursePrerequisite(student, cls);
            }

            if (cls.StudentClasses.Count() + studentIds.Count() > cls.LimitNumberStudent)
            {
                throw new BadHttpRequestException($"Lớp [{cls.Name}] Đã Đủ Chỉ Số", StatusCodes.Status400BadRequest);
            }
        }

        private async Task ValidateCoursePrerequisite(Student student, Class cls)
        {
            var currentCourseIdPrerRequiredList = (List<Guid>)await _unitOfWork.GetRepository<Course>()
                .SingleOrDefaultAsync(selector: x => x.CoursePrerequisites.Select(cp => cp.PrerequisiteCourseId),
                predicate: x => x.Classes.Any(c => c.Id.Equals(cls.Id)),
                include: x => x.Include(x => x.CoursePrerequisites));

            var allCoursePrerIdRequired = new List<Guid>();

            var allCoursePrerIdRequiredRender = await RenderAllCoursePrerRequired(currentCourseIdPrerRequiredList);
            allCoursePrerIdRequired.AddRange(allCoursePrerIdRequiredRender);

            if (allCoursePrerIdRequired?.Any() ?? false)
            {
                await ValidateCoursePrerProgress(student, cls, allCoursePrerIdRequired);
            }
        }

        private async Task ValidateCoursePrerProgress(Student student, Class cls, List<Guid> allCoursePrerIdRequired)
        {
            var courseRequiredList = new List<Course>();

            foreach (Guid id in allCoursePrerIdRequired)
            {
                var courseRequired = await _unitOfWork.GetRepository<Course>()
                   .SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x.Include(x => x.CoursePrerequisites));

                courseRequiredList.Add(courseRequired);
            }

            var courseCompleted = await _unitOfWork.GetRepository<Course>()
               .GetListAsync(predicate: x => x.Classes.Any(c => c.StudentClasses
               .Any(sc => sc.StudentId.Equals(student.Id) && c.Status!.Equals("COMPLETED"))));

            if (courseCompleted?.Any() ?? false)
            {
                var courseNotSatisfied = courseRequiredList.Where(cr => !courseCompleted.Any(c => cr.Id == c.Id)).ToList();
                if (courseNotSatisfied?.Any() ?? false)
                {
                    throw new BadHttpRequestException($"Học Sinh {student.FullName} Chưa Hoàn Thành Khóa Học Tiên Quyết " +
                        $"[ {string.Join(", ", courseNotSatisfied.Select(c => c.Name))} ] Để Tham Gia Vào Lớp [{cls.Name}]", StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                throw new BadHttpRequestException($"Học Sinh {student.FullName} Chưa Hoàn Thành Khóa Học Tiên Quyết " +
                       $"[ {string.Join(", ", courseRequiredList.Select(c => c.Name))} ] Để Tham Gia Vào Lớp [{cls.Name}]", StatusCodes.Status400BadRequest);
            }

        }

        private async Task<List<Guid>> RenderAllCoursePrerRequired(List<Guid> currentCourseIdPrerRequiredList)
        {
            var allCoursePrerIdRequired = new List<Guid>();

            if (currentCourseIdPrerRequiredList?.Any() ?? false)
            {
                allCoursePrerIdRequired.AddRange(currentCourseIdPrerRequiredList);

                currentCourseIdPrerRequiredList = await GetSubCoursePrerIdRequired(currentCourseIdPrerRequiredList);

                allCoursePrerIdRequired.AddRange(currentCourseIdPrerRequiredList);
            }

            return allCoursePrerIdRequired;
        }

        private async Task<List<Guid>> GetSubCoursePrerIdRequired(List<Guid> courseIdRequiredList)
        {
            var subCoursePrerIdRequiredList = new List<Guid>();

            bool isAll = false;

            while (isAll == false)
            {
                var tempCourseIdRequiredList = new List<Guid>();

                foreach (Guid id in courseIdRequiredList!)
                {
                    var coursePrerIdRequired = await _unitOfWork.GetRepository<Course>()
                       .SingleOrDefaultAsync(selector: x => x.CoursePrerequisites.Select(cp => cp.PrerequisiteCourseId),
                       predicate: x => x.CoursePrerequisites.Any(cp => cp.CurrentCourseId == id),
                       include: x => x.Include(x => x.CoursePrerequisites));

                    if (coursePrerIdRequired?.Any() ?? false)
                    {
                        tempCourseIdRequiredList.AddRange(coursePrerIdRequired);
                    }
                }
                courseIdRequiredList = tempCourseIdRequiredList;

                subCoursePrerIdRequiredList.AddRange(courseIdRequiredList);

                isAll = courseIdRequiredList.Any() ? false : true;
            }

            return subCoursePrerIdRequiredList ??= new List<Guid>();
        }

        private void ValidateSchedule(List<StudentScheduleResponse> allStudentSchedules, Class cls)
        {
            if (allStudentSchedules != null && allStudentSchedules.Count() > 0)
            {
                foreach (var ass in allStudentSchedules)
                {
                    foreach (var s in cls.Schedules)
                    {
                        if (ass.Date == s.Date && ass.StartTime == s.Slot!.StartTime)
                        {
                            throw new BadHttpRequestException($"Lịch Lớp Đang Học Hiên Tại [{ass.ClassName}] Của Học Sinh [{ass.StudentName}] Bị Trùng Thời Gian Bắt Đầu [{s.Slot.StartTime}]" +
                                $" Với Lịch Của Lớp [{cls.Name}]", StatusCodes.Status400BadRequest);
                        }
                    }
                }
            }
        }
        private async Task<double> CalculateTotal(List<CheckoutRequest> requests)
        {

            double total = 0.0;

            foreach (var request in requests)
            {
                var price = await _unitOfWork.GetRepository<Class>()
                .SingleOrDefaultAsync(selector: x => x.Course!.Price, predicate: x => x.Id.Equals(request.ClassId), include: x => x.Include(x => x.Course)!);

                total += request.StudentIdList.Count() * price;
            }

            return total;
        }

        private double CalculateDiscountEachItem(int numberItem, double total)
        {
            double discount = 0.0;

            if (numberItem == 2)
            {
                discount = double.Round((total * 10) / 100 / numberItem);
            }

            if (numberItem >= 3)
            {
                int percent = 10 + (numberItem - 2) * 5;
                percent = percent > 30 ? 30 : percent;

                discount = double.Round((total * percent) / 100 / numberItem);
            }

            return discount;
        }

        private async Task<string> GenerateStudentNameString(List<Guid> stundentIdList)
        {
            var studentNameList = new List<string>();

            foreach (Guid id in stundentIdList)
            {
                var studentName = await _unitOfWork.GetRepository<Student>()
                .SingleOrDefaultAsync(selector: x => x.FullName, predicate: x => x.Id.Equals(id));

                studentNameList.Add(studentName!);
            }

            return string.Join(", ", studentNameList);
        }
        public async Task<BillTopUpResponse?> GenerateBillTopUpTransactionAsync(Guid id)
        {
            var transaction = await _unitOfWork.GetRepository<WalletTransaction>().SingleOrDefaultAsync(predicate: x => x.Id == id);
            if (transaction == null)
            {
                throw new BadHttpRequestException($"Id [{id}] Của Đơn Hàng Không Tồn Tại Trong Hệ Thống", StatusCodes.Status400BadRequest);
            }
            if (transaction.Status == TransactionStatusEnum.Processing.ToString())
            {
                return default;
            }
            string status = transaction.Status == TransactionStatusEnum.Success.ToString() ? TransactionStatusMessageConstant.Success : TransactionStatusMessageConstant.Failed;

            var response = new BillTopUpResponse
            {
                Status = status,
                Message = transaction.Description!,
                MoneyAmount = transaction.Money,
                Currency = transaction.Currency,
                Date = transaction.UpdateTime!.Value,
                Method = transaction.Method!,
                Type = transaction.Type!,
                Customer = transaction.CreateBy!,
            };

            return response;
        }

        public async Task<(Guid, string)> GenerateTopUpTransAsync(double money)
        {
            try
            {
                string txnRefCode = StringHelper.GenerateTransactionTxnRefCode(TransactionTypeEnum.TopUp);

                var currentUser = await GetUserFromJwt();
                var transactionId = Guid.NewGuid();

                var transaction = new WalletTransaction
                {
                    Id = transactionId,
                    TransactionCode = string.Empty,
                    Money = money,
                    Type = TransactionTypeEnum.TopUp.ToString(),
                    Method = TransactionMethodEnum.Vnpay.ToString(),
                    Description = "Nạp Tiền Vào Ví",
                    Status = TransactionStatusEnum.Processing.ToString(),
                    CreateBy = currentUser.FullName,
                    CreateTime = DateTime.Now,
                    Signature = txnRefCode,
                    PersonalWalletId = currentUser.PersonalWalletId!.Value,
                };

                await _unitOfWork.GetRepository<WalletTransaction>().InsertAsync(transaction);
                await _unitOfWork.CommitAsync();

                return (transactionId, txnRefCode);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hễ Thống Phát Sinh: [{ex.Message}] Inner Exception: [{ex.InnerException!}]");
            }
        }

        public async Task<(string, bool)> HandelSuccessReturnDataVnpayAsync(string transactionCode, string txnRefCode, TransactionTypeEnum type)
        {
            try
            {
                var gatewayTransactions = new List<WalletTransaction>();
                if (type == TransactionTypeEnum.TopUp)
                {
                    gatewayTransactions = (await _unitOfWork.GetRepository<WalletTransaction>().GetListAsync(predicate: x => x.Signature == txnRefCode)).ToList();
                    if (gatewayTransactions == null)
                    {
                        return ("Giao Dịch Sử Lý Không Tồn Tại Trong Hệ Thống Vui Lòng Thực Hiện Lại", false);
                    }

                    var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate: x => x.Id == gatewayTransactions[0].PersonalWalletId);

                    foreach (var trans in gatewayTransactions)
                    {
                        personalWallet.Balance += trans.Money;
                    }

                    _unitOfWork.GetRepository<PersonalWallet>().UpdateAsync(personalWallet);
                }
                if (type == TransactionTypeEnum.Payment)
                {
                    gatewayTransactions = (await _unitOfWork.GetRepository<WalletTransaction>()
                        .GetListAsync(predicate: x => x.Status == TransactionStatusEnum.Processing.ToString())).ToList();

                    gatewayTransactions = gatewayTransactions.Where(gt => gt.Signature!.Substring(0, Math.Min(36, gt.Signature.Length)).Trim().Equals(txnRefCode.Trim())).ToList();

                    if (gatewayTransactions == null)
                    {
                        return ("Giao Dịch Sử Lý Không Tồn Tại Trong Hệ Thống Vui Lòng Thực Hiện Lại", false);
                    }

                    foreach (var transaction in gatewayTransactions)
                    {
                        var result = StringHelper.ExtractAttachValueFromSignature(transaction.Signature!);

                        await InsertAttachValue(result);
                    }
                }
                UpdateTransaction(gatewayTransactions, type, transactionCode, true);

                _unitOfWork.Commit();
                return (string.Empty, true);
            }
            catch (Exception ex)
            {
                return ($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", false);
            }
        }

        private async Task InsertAttachValue(Dictionary<string, List<string>> result)
        {
            Guid classId = default;

            foreach (var pair in result)
            {
                if (pair.Key == TransactionAttachValueEnum.ClassId.ToString())
                {
                    classId = Guid.Parse(pair.Value[0]);
                    continue;
                }
                if (pair.Key == TransactionAttachValueEnum.StudentId.ToString())
                {
                    var studentAttendanceList = await RenderStudentAttendanceList(classId, pair.Value.Select(v => Guid.Parse(v)).ToList());
                    var studentClassList = pair.Value.Select(v => Guid.Parse(v)).ToList().Select(id =>
                    new StudentClass
                    {
                        Id = new Guid(),
                        StudentId = id,
                        ClassId = classId,
                    }).ToList();

                    await _unitOfWork.GetRepository<Attendance>().InsertRangeAsync(studentAttendanceList);
                    await _unitOfWork.GetRepository<StudentClass>().InsertRangeAsync(studentClassList);
                    continue;
                }
                if (pair.Key == TransactionAttachValueEnum.CartItemId.ToString())
                {
                    var cartItemId = pair.Value.Select(v => Guid.Parse(v)).ToList();

                    foreach (Guid id in cartItemId)
                    {
                        _unitOfWork.GetRepository<CartItem>().DeleteAsync(await _unitOfWork.GetRepository<CartItem>().SingleOrDefaultAsync(predicate: x => x.Id == id));
                    }
                    continue;
                }
            }
        }

        private void UpdateTransaction(List<WalletTransaction> transactions, TransactionTypeEnum type, string transactionCodeReturn, bool isSuccess)
        {
            var storedCode = new List<string>();
            Random random = new Random();
            string extraCode = string.Empty;

            string chars = "0123456789";
            int numberDigit = isSuccess == true ? 1 : 8;
            string startCode = type == TransactionTypeEnum.TopUp ? "12" : type == TransactionTypeEnum.Payment ? "11" : "10";

            foreach (var trans in transactions)
            {
                do
                {
                    extraCode = new string(Enumerable.Repeat(chars, numberDigit).Select(s => s[random.Next(s.Length)]).ToArray());
                } while (storedCode.Any(sno => sno == extraCode));
                storedCode.Add(extraCode);

                trans.TransactionCode = startCode + extraCode + transactionCodeReturn;
                trans.Status = isSuccess == true ? TransactionStatusEnum.Success.ToString() : TransactionStatusEnum.Failed.ToString();
                trans.UpdateTime = DateTime.Now;
            }
            _unitOfWork.GetRepository<WalletTransaction>().UpdateRange(transactions);

        }

        public async Task<(string, bool)> HandelFailedReturnDataVnpayAsync(string transactionCode, string txnRefCode, TransactionTypeEnum type)
        {
            try
            {
                var gatewayTransactions = (await _unitOfWork.GetRepository<WalletTransaction>()
                   .GetListAsync(predicate: x => x.Status == TransactionStatusEnum.Processing.ToString())).ToList();

                gatewayTransactions.Where(gt => gt.Signature!.Substring(0, Math.Min(36, gt.Signature.Length)).Trim().Equals(txnRefCode.Trim()));

                if (gatewayTransactions == null)
                {
                    return ("Giao Dịch Sử Lý Không Tồn Tại Trong Hệ Thống Vui Lòng Thực Hiện Lại", false);
                }

                UpdateTransaction(gatewayTransactions, type, transactionCode, false);

                _unitOfWork.Commit();

                return (string.Empty, true);
            }
            catch (Exception ex)
            {
                return ($"Lỗi Hệ Thống Phát Sinh [{ex.Message}]", false);
            }
        }

        public async Task<(string, double)> GeneratePaymentTransAsync(List<ItemGenerate> items)
        {
            var currentPayer = await GetUserFromJwt();

            double total = await ConvertItemAndGetTotal(items);
            double discountEachItem = CalculateDiscountEachItem(items.Count(), total);

            string txnRefCode = StringHelper.GenerateTransactionTxnRefCode(TransactionTypeEnum.Payment);

            var transactions = new List<WalletTransaction>();
            try
            {
                foreach (var item in items)
                {
                    var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                        predicate: x => x.Id == item.ClassId,
                        include: x => x.Include(x => x.Course!)
                        .Include(x => x.Schedules)
                        .Include(x => x.StudentClasses));

                    string signature = txnRefCode + StringHelper.GenerateAttachValueForTxnRefCode(item);

                    var currentRequestTotal = cls.Course!.Price * item.StudentIdList.Count();
                    var studentNameString = await GenerateStudentNameString(item.StudentIdList);

                    var newTransaction = new WalletTransaction
                    {
                        Id = new Guid(),
                        TransactionCode = string.Empty,
                        Money = currentRequestTotal,
                        Discount = discountEachItem,
                        Type = TransactionTypeEnum.Payment.ToString(),
                        Method = TransactionMethodEnum.Vnpay.ToString(),
                        Description = $"Đăng Ký Học Sinh {studentNameString} Vào Lớp {cls.Name}",
                        CreateTime = DateTime.Now,
                        PersonalWalletId = currentPayer.PersonalWalletId!.Value,
                        CreateBy = currentPayer.FullName,
                        Signature = signature,
                        Status = TransactionStatusEnum.Processing.ToString(),
                    };

                    transactions.Add(newTransaction);
                }

                await _unitOfWork.GetRepository<WalletTransaction>().InsertRangeAsync(transactions);
                _unitOfWork.Commit();

                return (txnRefCode, total);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hệ Thống Phát Sinh: [{ex.Message}] Inner Exception: [{ex.InnerException!}]");
            }
        }

        private async Task<double> ConvertItemAndGetTotal(List<ItemGenerate> items)
        {
            double total = 0.0;
            var requests = new List<CheckoutRequest>();

            foreach (var item in items)
            {
                requests.Add(new CheckoutRequest
                {
                    ClassId = item.ClassId,
                    StudentIdList = item.StudentIdList,
                });
                total = await CalculateTotal(requests);
            }

            return total;
        }

        public async Task<BillPaymentResponse?> GenerateBillPaymentTransactionAssync(string txnRefCode)
        {
            var transactions = (await _unitOfWork.GetRepository<WalletTransaction>().GetListAsync()).ToList();
            transactions = transactions.Where(gt => gt.Signature!.Substring(0, Math.Min(36, gt.Signature.Length)).Trim().Equals(txnRefCode.Trim())).ToList();

            if (transactions == null || transactions.Count() == 0)
            {
                throw new BadHttpRequestException($"TxnRefCode [{txnRefCode}] Không Tồn Tại Trong Hệ Thống", StatusCodes.Status400BadRequest);
            }
            if (transactions.Any(trans => trans.Status == TransactionStatusEnum.Processing.ToString()))
            {
                return default;
            }

            string status = transactions.Any(trans => trans.Status == TransactionStatusEnum.Failed.ToString()) ? TransactionStatusMessageConstant.Failed : TransactionStatusMessageConstant.Success;
            double totalAmount = 0.0, totalDiscount = 0.0;
            foreach (var trans in transactions)
            {
                totalAmount += trans.Money;
                totalDiscount += trans.Discount;
            }

            var response = new BillPaymentResponse
            {
                Status = status,
                Message = string.Join(", ", transactions.Select(trans => trans.Description).ToList()),
                MoneyAmount = totalAmount,
                Discount = totalDiscount,
                MoneyPaid = totalAmount - totalDiscount,
                Date = DateTime.Now,
                Method = TransactionMethodEnum.Vnpay.ToString(),
                Type = TransactionTypeEnum.Payment.ToString(),
                Payer = transactions[0].CreateBy!,
            };

            return response;
        }

        public async Task<List<RevenueResponse>> GetRevenueTransactionByTimeAsync(RevenueTimeEnum time)
        {
            var transactions = await _unitOfWork.GetRepository<WalletTransaction>()
                .GetListAsync(predicate: x => x.Type != TransactionTypeEnum.TopUp.ToString(), orderBy: x => x.OrderBy(x => x.CreateTime));

            var transactionGroupTimes = GetTransactionGroupTimes(transactions, time);

            var revenueGroupTimes = transactionGroupTimes!.Select((group, index) => new RevenueResponse
            {
                Number = index + 1,
                StartFrom = group.First().CreateTime,
                EndAt = group.Last().CreateTime,
                TotalMoneyEarn = group.Sum(t => t.Type == TransactionTypeEnum.Payment.ToString() ? t.Money : 0),
                TotalMoneyDiscount = group.Sum(t => t.Discount),
                TotalMoneyRefund = group.Sum(t => t.Type == TransactionTypeEnum.Refund.ToString() ? t.Money : 0),
                TotalRevenue = group.Sum(t => t.Type == TransactionTypeEnum.Payment.ToString() ? t.Money : -t.Money) - group.Sum(t => t.Discount),
            }).ToList();

            return revenueGroupTimes;
        }

        private IEnumerable<IGrouping<int, WalletTransaction>>? GetTransactionGroupTimes(ICollection<WalletTransaction> transactions, RevenueTimeEnum time)
        {
            switch (time)
            {
                case RevenueTimeEnum.Week:
                    return transactions.GroupBy(trans => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(trans.CreateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday));
                case RevenueTimeEnum.Month:
                    return transactions.GroupBy(trans => trans.CreateTime.Month);
                case RevenueTimeEnum.Quarter:
                    return transactions.ToLookup(trans => (trans.CreateTime.Month - 1) / 3 + 1, trans => trans).OrderBy(group => group.Key);
                case RevenueTimeEnum.Year:
                    return transactions.GroupBy(trans => trans.CreateTime.Year);
                default:
                    break;
            }
            return default;
        }
    }
}
