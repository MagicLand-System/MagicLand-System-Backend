using AutoMapper;
using Azure.Core;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Cart;
using MagicLand_System.PayLoad.Response.Student;
using MagicLand_System.PayLoad.Response.User;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace MagicLand_System.Services.Implements
{
    public class UserService : BaseService<UserService>, IUserService
    {
        public UserService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<UserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<LoginResponse> Authentication(LoginRequest loginRequest)
        {
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
                throw new BadHttpRequestException("date of birth is previous now", StatusCodes.Status400BadRequest);
            }
            User user = new User
            {
                DateOfBirth = registerRequest.DateOfBirth,
                Email = registerRequest.Email,
                FullName = registerRequest.FullName,
                Gender = registerRequest.Gender,
                Phone = registerRequest.Phone,
                RoleId = role,
                District = registerRequest.District,
                Street = registerRequest.Street,
                City = registerRequest.City,
                Id = Guid.NewGuid(),
            };
            await _unitOfWork.GetRepository<User>().InsertAsync(user);
            var isUserSuccess = await _unitOfWork.CommitAsync() > 0;
            if (!isUserSuccess)
            {
                throw new BadHttpRequestException("user can't insert", StatusCodes.Status400BadRequest);
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
                throw new BadHttpRequestException("cart can't insert", StatusCodes.Status400BadRequest);
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
                    Id = user.Id,
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

        public async Task<BillResponse> CheckoutNowAsync(CheckoutRequest request)
        {
            var price = await _unitOfWork.GetRepository<Course>()
                .SingleOrDefaultAsync(selector: x => x.Price, predicate: x => x.Classes.Any(c => c.Id.Equals(request.ClassId)));

            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>()
                .SingleOrDefaultAsync(predicate: x => x.UserId.Equals(GetUserIdFromJwt()));

            double total = ValidateWallet(request.StudentsIdList.Count(), price, personalWallet.Balance);

            await PurchaseProgress(request, personalWallet, total);

            var currentPayer = await GetCurrentUser();

            var bill = new BillResponse
            {
                Status = "Purchase Success",
                Message = "Students has been registered in class",
                Cost = total,
                Discount = 0.0,
                MoneyPaid = total,
                Date = DateTime.Now,
                Method = CheckOutMethodEnum.SystemWallet.ToString(),
                Payer = currentPayer.FullName!,              
            };

            return bill;
        }

        private async Task PurchaseProgress(CheckoutRequest request, PersonalWallet personalWallet, double total)
        {
            try
            {
                var newTransaction = new WalletTransaction
                {
                    Id = new Guid(),
                    Money = total,
                    Type = CheckOutMethodEnum.SystemWallet.ToString(),
                    Description = $"Direct registered students into class: {request.ClassId}",
                    CreatedTime = DateTime.Now,
                    PersonalWalletId = personalWallet.Id,
                    PersonalWallet = personalWallet
                };

                var studentInClasses = request.StudentsIdList.Select(sil =>
                new StudentClass
                {
                    Id = new Guid(),
                    Status = "NORMAL",
                    StudentId = sil,
                    ClassId = request.ClassId,
                }).ToList();

                personalWallet.Balance = personalWallet.Balance - total;

                _unitOfWork.GetRepository<PersonalWallet>().UpdateAsync(personalWallet);
                await _unitOfWork.GetRepository<WalletTransaction>().InsertAsync(newTransaction);
                await _unitOfWork.GetRepository<StudentClass>().InsertRangeAsync(studentInClasses);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException!.ToString());
            }
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

            //Validate Course Prerequsite  ?

            return true;
        }

        private async Task ValidateSuitableClass(List<Guid> studentIds, Class cls)
        {
            foreach (Guid id in studentIds)
            {
                var student = await _unitOfWork.GetRepository<Student>()
                .SingleOrDefaultAsync(predicate: x => x.Id.Equals(id));

                if(cls.StudentClasses.Any(sc => sc.StudentId.Equals(id)))
                {
                    throw new BadHttpRequestException($"Student {student.FullName} already assigned to class {cls.Name}", StatusCodes.Status400BadRequest);
                }

                int age = DateTime.Now.Year - student.DateOfBirth.Year;

                if (age > cls.Course!.MaxYearOldsStudent || age < cls.Course.MinYearOldsStudent)
                {
                    throw new BadHttpRequestException($"Student {student.FullName} age is not suitable to asign class {cls.Name}", StatusCodes.Status400BadRequest);
                }
            }

            if (cls.StudentClasses.Count() + studentIds.Count() > cls.LimitNumberStudent)
            {
                throw new BadHttpRequestException($"Class {cls.Name} has over student index", StatusCodes.Status400BadRequest);
            }
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
                            throw new BadHttpRequestException($"Current class schedule of {ass.StudentName} is coincide start time {s.Slot.StartTime} with {cls.Name} schedule slot", StatusCodes.Status400BadRequest);
                        }
                    }
                }
            }
        }

        private double ValidateWallet(int numberStudent, double price, double balance)
        {
            double totalPrice = price * numberStudent;

            if (balance < totalPrice)
            {
                throw new BadHttpRequestException($"Your balance has not enough balance require greater than: {totalPrice}", StatusCodes.Status400BadRequest);
            }

            return totalPrice;
        }


    }
}
