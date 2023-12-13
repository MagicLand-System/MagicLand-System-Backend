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
            // Address address = new Address
            // {
            //     Street = registerRequest.Street,
            //     City = registerRequest.City,
            //     District = registerRequest.District,
            //     Id = Guid.NewGuid(),
            // };
            //await _unitOfWork.GetRepository<Address>().InsertAsync(address);
            //var isAddressSuccess = await _unitOfWork.CommitAsync() > 0;
            //if(!isAddressSuccess)
            //{
            //     throw new BadHttpRequestException("address can't insert", StatusCodes.Status400BadRequest);
            //}
            //var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(predicate : x => x.Name.Equals(RoleEnum.PARENT.GetDescriptionFromEnum<RoleEnum>()),selector : x => x.Id);
            //if(registerRequest.DateOfBirth > DateTime.Now)
            // {
            //     throw new BadHttpRequestException("date of birth is previous now", StatusCodes.Status400BadRequest);
            // }
            //User user = new User
            //{
            //    AddressId = address.Id,
            //    DateOfBirth = registerRequest.DateOfBirth,
            //    Email = registerRequest.Email,
            //    FullName = registerRequest.FullName,
            //    Gender = registerRequest.Gender,
            //    Phone = registerRequest.Phone,
            //    RoleId = role,
            //    Id = Guid.NewGuid(),
            //};
            //await _unitOfWork.GetRepository<User>().InsertAsync(user);
            //var isUserSuccess = await _unitOfWork.CommitAsync() > 0;
            //if (!isUserSuccess)
            //{
            //    throw new BadHttpRequestException("user can't insert", StatusCodes.Status400BadRequest);
            //}
            //Cart cart = new Cart
            //{
            //    Id = Guid.NewGuid(),
            //    UserId = user.Id,
            //};
            //await _unitOfWork.GetRepository<Cart>().InsertAsync(cart);
            //var isCartSuccess = await _unitOfWork.CommitAsync() > 0;
            //if (!isCartSuccess)
            //{
            //    throw new BadHttpRequestException("cart can't insert", StatusCodes.Status400BadRequest);
            //}
            //PersonalWallet personalWallet = new PersonalWallet
            //{
            //    Id = Guid.NewGuid(),
            //    UserId = user.Id,
            //    Balance = 0
            //};
            //user.CartId = cart.Id;
            //user.PersonalWalletId = personalWallet.Id;
            //_unitOfWork.GetRepository<User>().UpdateAsync(user);
            //await _unitOfWork.GetRepository<PersonalWallet>().InsertAsync(personalWallet);
            //var isSuccess = await _unitOfWork.CommitAsync() > 0;
            return true; //isSuccess;
        }

        public async Task<BillResponse> CheckoutNowAsync(CheckoutRequest request)
        {
            var price = await _unitOfWork.GetRepository<Course>()
                .SingleOrDefaultAsync(selector: x => x.Price, predicate: x => x.Classes.Any(c => c.Id.Equals(request.ClassId)));

            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>()
                .SingleOrDefaultAsync(predicate: x => x.UserId.Equals(GetUserIdFromJwt()));


            var currentPayer = await GetCurrentUser();

            var total = CalculateTotal(request.StudentsIdList.Count(), price);

            try
            {
                var newTransaction = new WalletTransaction
                {
                    Id = new Guid(),
                    Money = total,
                    Type = CheckOutMethodEnum.SystemWallet.ToString(),
                    Description = "Direct registered class",
                    CreatedTime = DateTime.Now,
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

            var bill = new BillResponse
            {
                Status = "Purchase Success",
                Date = DateTime.Now,
                Method = CheckOutMethodEnum.SystemWallet,
                Payer = currentPayer.FullName!,
                MoneyPaid = total,
            };

            return bill;
        }


        public async Task<bool> ValidRegisterAsync(List<StudentScheduleResponse> allStudentSchedules, Guid classId, List<Guid> studentIds)
        {
            var course = await _unitOfWork.GetRepository<Course>()
                .SingleOrDefaultAsync(predicate: x => x.Classes.Any(c => c.Id.Equals(classId)), include: x => x
                .Include(x => x.Classes)
                .ThenInclude(c => c.Schedules)
                .ThenInclude(s => s.Slot)
                .Include(x => x.Classes)
                .ThenInclude(c => c.StudentClasses));

            var cls = course.Classes.SingleOrDefault(c => c.Id.Equals(classId))!;

            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>()
                .SingleOrDefaultAsync(predicate: x => x.UserId.Equals(GetUserIdFromJwt()));

            var total = CalculateTotal(studentIds.Count(), course.Price);

            if (allStudentSchedules != null && allStudentSchedules.Count() > 0)
            {
                //Checking Schedule
            }

            //Checking Course Prerequsite  ?

            foreach (var id in studentIds)
            {
                var student = await _unitOfWork.GetRepository<Student>()
                .SingleOrDefaultAsync(predicate: x => x.Id.Equals(id));

                var age = DateTime.Now.Year - student.DateOfBirth.Year;
                if (age > course.MaxYearOldsStudent || age < course.MinYearOldsStudent)
                {
                    throw new BadHttpRequestException($"Student {student.FullName} is not suitable to asign class {cls.Name}", StatusCodes.Status400BadRequest);
                }

                if (cls.StudentClasses.Any(sc => sc.StudentId.Equals(id)))
                {
                    throw new BadHttpRequestException($"Student {student.FullName} is already asign class {cls.Name}", StatusCodes.Status400BadRequest);
                }
            }

            if (cls.StudentClasses.Count() + studentIds.Count() > cls.LimitNumberStudent)
            {
                throw new BadHttpRequestException($"Class {cls.Name} has over student index", StatusCodes.Status400BadRequest);
            }

            if (personalWallet.Balance < total)
            {
                throw new BadHttpRequestException("Your balance has not enough", StatusCodes.Status400BadRequest);
            }
            return true;
        }

        protected double CalculateTotal(int numberStudent, double price)
        {
            var totalPrice = price * numberStudent;
            var actualTotal = totalPrice; //Apply Promotion/Sales
            return actualTotal;
        }
    }
}
