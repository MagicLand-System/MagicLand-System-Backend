﻿using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Request.User;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class UserService : BaseService<UserService>, IUserService
    {
        public UserService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<UserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<LoginResponse> Authentication(LoginRequest loginRequest)
        {
            var date = DateTime.Now;
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: u => u.Phone.Trim().Equals(loginRequest.Phone.Trim()), include: u => u.Include(u => u.Role));
            if (user == null)
            {
                return null;
            }
            string Role = user.Role.Name;
            Tuple<string, Guid> guidClaim = new Tuple<string, Guid>("userId", user.Id);
            var token = JwtUtil.GenerateJwtToken(user, guidClaim);
            LoginResponse loginResponse = new LoginResponse
            {
                Role = Role,
                AccessToken = token,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                FullName = user.FullName,
                Gender = user.Gender,
                Phone = user.Phone,
            };
            return loginResponse;
        }

        public async Task<bool> CheckUserExistByPhone(string phone)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Phone.Trim().Equals(phone.Trim()));
            if (user == null)
            {
                return false;
            }
            return true;
        }

        public async Task<User> GetCurrentUser()
        {
            var account = await GetUserFromJwt();
            return account;
        }

        public async Task<List<User>> GetUsers()
        {
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(predicate: x => x.Id == x.Id);
            return users.ToList();
        }

        public async Task<NewTokenResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            var userId = JwtUtil.ReadToken(refreshTokenRequest.OldToken);
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(userId), include: u => u.Include(u => u.Role));
            Tuple<string, Guid> guidClaim = new Tuple<string, Guid>("userId", user.Id);
            var token = JwtUtil.GenerateJwtToken(user, guidClaim);
            return new NewTokenResponse { Token = token };
        }

        public async Task<bool> RegisterNewUser(RegisterRequest registerRequest)
        {
            var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(predicate: x => x.Name.Equals(RoleEnum.PARENT.GetDescriptionFromEnum<RoleEnum>()), selector: x => x.Id);
            if (registerRequest.DateOfBirth > DateTime.Now)
            {
                throw new BadHttpRequestException("Ngày sinh phải trước ngày hiện tại", StatusCodes.Status400BadRequest);
            }
            User user = new User
            {
                DateOfBirth = registerRequest.DateOfBirth,
                Email = registerRequest.Email,
                FullName = registerRequest.FullName,
                Gender = registerRequest.Gender,
                Phone = registerRequest.Phone,
                RoleId = role,
                Address = registerRequest.Address,
                Id = Guid.NewGuid(),
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(user);
            var isUserSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isUserSuccess)
            {
                throw new BadHttpRequestException("Không thể thêm user này", StatusCodes.Status400BadRequest);
            }
            Cart cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
            };
            await _unitOfWork.GetRepository<Cart>().InsertAsync(cart);
            var isCartSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isCartSuccess)
            {
                throw new BadHttpRequestException("Không thể thêm user này", StatusCodes.Status400BadRequest);
            }
            PersonalWallet personalWallet = new PersonalWallet
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Balance = 0
            };
            user.CartId = cart.Id;
            user.PersonalWalletId = personalWallet.Id;
            _unitOfWork.GetRepository<User>().UpdateAsync(user);
            await _unitOfWork.GetRepository<PersonalWallet>().InsertAsync(personalWallet);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            return true; //isSuccess;
        }
        public async Task<List<LecturerResponse>> GetLecturers()
        {
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(include: x => x.Include(x => x.Role));
            if (users == null)
            {
                return null;
            }
            var lecturers = users.Where(x => x.Role.Name.Equals(RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>()));
            List<LecturerResponse> lecturerResponses = new List<LecturerResponse>();
            foreach (var user in lecturers)
            {
                LecturerResponse response = new LecturerResponse
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    AvatarImage = user.AvatarImage,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    Phone = user.Phone,
                    LectureId = user.Id,
                    Role = RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>()
                };
                lecturerResponses.Add(response);
            }
            if (lecturerResponses.Count == 0)
            {
                return null;
            }
            return lecturerResponses;
        }

        public async Task<BillResponse> CheckoutAsync(List<CheckoutRequest> requests)
        {
            var currentPayer = await GetCurrentUser();
            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate: x => x.UserId.Equals(GetUserIdFromJwt()));

            await ValidateScheduleEachItem(requests.Select(r => r.ClassId).ToList());

            double total = await ValidateBalance(requests, personalWallet.Balance);
            double discountEachItem = CalculateDiscountEachItem(requests.Count(), total);

            var messageList = await PurchaseProgress(requests, personalWallet, currentPayer, discountEachItem);

            return RenderBill(currentPayer, messageList, total, discountEachItem * requests.Count());
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

                var currentRequestTotal = cls.Course!.Price * request.StudentIdList.Count() - discountEachItem;

                string studentNameString = await RenderStudentNameString(request.StudentIdList);

                var newStudentAttendanceList = await RenderStudentAttendanceList(cls, request.StudentIdList);

                var newTransaction = new WalletTransaction
                {
                    Id = new Guid(),
                    TransactionCode = StringHelper.GenerateRadomTransactionCode(TransactionTypeEnum.Payment),
                    Money = currentRequestTotal,
                    Type = TransactionTypeEnum.Payment.ToString(),
                    Method = TransactionMethodEnum.SystemWallet.ToString(),
                    Description = $"Đăng Ký Học Sinh {studentNameString} Vào Lớp {cls.Name}",
                    SystemDescription = $"[ClassCodes: {cls.ClassCode} ] [Parent: {currentPayer.FullName} ] [StudentNames: {studentNameString}]",
                    CreatedTime = DateTime.Now,
                    PersonalWalletId = personalWallet.Id,
                    PersonalWallet = personalWallet,
                    IsProcessed = false,
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
                    await UpdateTransaction(cls);

                    await UpdateStudentAttendance(cls);

                    newStudentAttendanceList.ForEach(attendance => attendance.IsPublic = true);
                    newTransaction.IsProcessed = true;
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

        private async Task UpdateTransaction(Class cls)
        {
            try
            {
                var oldTransactions = await _unitOfWork.GetRepository<WalletTransaction>()
                .GetListAsync(predicate: x => x.SystemDescription!.Contains(cls.ClassCode!));

                if (oldTransactions.Any())
                {
                    oldTransactions.ToList().ForEach(transaction => transaction.IsProcessed = true);
                    _unitOfWork.GetRepository<WalletTransaction>().UpdateRange(oldTransactions);

                    await _unitOfWork.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi Hệ Thống Phát Sinh Khi Sử Lý Thanh Toán Lớp [{cls.Name}]" + ex.InnerException!.ToString());
            }
          
        }

        private BillResponse RenderBill(User currentPayer, List<string> messageList, double total, double discount)
        {
            var bill = new BillResponse
            {
                Status = "Thanh Toán Thành Công",
                Message = string.Join(" , ", messageList),
                Cost = total,
                Discount = discount,
                MoneyPaid = total - discount,
                Date = DateTime.Now,
                Method = TransactionMethodEnum.SystemWallet.ToString(),
                Payer = currentPayer.FullName!,
            };

            return bill;
        }


        private async Task<List<Attendance>> RenderStudentAttendanceList(Class cls, List<Guid> studentIds)
        {
            var studentAttendanceList = new List<Attendance>();

            var classSchedules = await _unitOfWork.GetRepository<Schedule>().GetListAsync(predicate: x => x.Class!.Id == cls.Id);

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
                throw new BadHttpRequestException($"Xin Lỗi Bạn Chỉ Có Thê Đăng Ký Lớp [Sắp Bắt Đầu], Lớp Này [{status}]",
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
        private async Task ValidateScheduleEachItem(List<Guid> classIdList)
        {
            var classes = new List<Class>();

            foreach(var id in classIdList)
            {
                var cls = await _unitOfWork.GetRepository<Class>()
                   .SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x
                   .Include(x => x.Schedules)
                   .ThenInclude(s => s.Slot)
                   .Include(x => x.Schedules)
                   .ThenInclude(s => s.Room)!);

                classes.Add(cls);
            }
           
            for(int i = 0; i < classes.Count - 1; i++)
            {
                for(int j = i + 1; j < classes.Count; j++)
                {
                    CheckConflictSchedule(classes, i, j);
                }
            }
        }

        private static void CheckConflictSchedule(List<Class> classes, int defaultIndex, int browserIndex)
        {
            var defaultSchedules = classes[defaultIndex].Schedules;
            var browserSchedules = classes[browserIndex].Schedules;

            foreach (var ds in defaultSchedules)
            {
                foreach (var bs in browserSchedules)
                {
                    if (ds.Date == bs.Date && ds.Slot!.StartTime == bs.Slot!.StartTime)
                    {
                        if (classes[defaultIndex].Id == classes[browserIndex].Id)
                        {
                            throw new BadHttpRequestException($"Bạn Đang Đăng Ký Lớp [{classes[defaultIndex].Name}] Nhiều Hơn 2 Lần", StatusCodes.Status400BadRequest);
                        }

                        throw new BadHttpRequestException($"Lịch Của Lớp [{classes[defaultIndex].Name}] Bị Trùng Thời Gian Bắt Đầu [{ds.Slot.StartTime}]" +
                        $" Với Lớp [{classes[browserIndex].Name}] Hãy Chọn Lớp Bạn Mong Muốn Đăng Ký Nhất", StatusCodes.Status400BadRequest);
                    }
                }
            }
        }

        private async Task<double> ValidateBalance(List<CheckoutRequest> requests, double balance)
        {

            double total = 0.0;

            foreach (var request in requests)
            {
                var price = await _unitOfWork.GetRepository<Class>()
                .SingleOrDefaultAsync(selector: x => x.Course!.Price, predicate: x => x.Id.Equals(request.ClassId), include: x => x.Include(x => x.Course)!);

                total += request.StudentIdList.Count() * price;
            }

            if (balance < total)
            {
                throw new BadHttpRequestException($"Số Dư Không Đủ, Yều Cầu Tài Khoản Có Ít Nhất: [{total} d]", StatusCodes.Status400BadRequest);
            }

            return total;
        }

        private double CalculateDiscountEachItem(int numberItem, double total)
        {
            double discount = numberItem >= 2
                   ? double.Round((total * 10) / 100 / numberItem)
                   : 0.0;

            return discount;
        }

        private async Task<string> RenderStudentNameString(List<Guid> stundentIdList)
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

        public async Task<UserResponse> UpdateUserAsync(UserRequest request)
        {
            try
            {
                var currentUser = await GetUserFromJwt();

                if (request.FullName != null)
                {
                    await UpdateCurrentUserTransaction(request, currentUser);

                    currentUser.FullName = request.FullName!;
                }

                currentUser.DateOfBirth = request.DateOfBirth != default ? request.DateOfBirth : currentUser.DateOfBirth;
                currentUser.Gender = request.Gender ?? currentUser.Gender;
                currentUser.AvatarImage = request.AvatarImage ?? currentUser.AvatarImage;
                currentUser.Email = request.Email ?? currentUser.Email;
                currentUser.Address = request.Address;

                _unitOfWork.GetRepository<User>().UpdateAsync(currentUser);
                await _unitOfWork.CommitAsync();

                return _mapper.Map<UserResponse>(currentUser);

            }
            catch(Exception ex)
            {
                throw new BadHttpRequestException($"Lỗi Hệ Thống Phát Sinh [{ex}]", StatusCodes.Status400BadRequest);
            }
        }

        private async Task UpdateCurrentUserTransaction(UserRequest request, User currentUser)
        {
            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate: x => x.UserId == currentUser.Id);

            var oldTransactions = await _unitOfWork.GetRepository<WalletTransaction>()
               .GetListAsync(predicate: x => x.PersonalWalletId == personalWallet.Id);

            foreach (var trans in oldTransactions)
            {
                var description = trans.SystemDescription!;
                if(description != null)
                {
                    trans.SystemDescription = StringHelper.UpdatePartValueOfTransactionDescription(description, request.FullName!, TransactionDescriptionEnum.Parent.ToString());
                    trans.PersonalWalletId = personalWallet.Id;
                    trans.PersonalWallet = personalWallet;
                }
            }

            _unitOfWork.GetRepository<WalletTransaction>().UpdateRange(oldTransactions);
        }
    }
}
