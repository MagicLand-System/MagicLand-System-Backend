using AutoMapper;
using Azure.Core;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MagicLand_System.Services.Implements
{
    public class UserService : BaseService<UserService>, IUserService
    {
        public UserService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<UserService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }
        
        public async Task<LoginResponse> Authentication(LoginRequest loginRequest)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate : u => u.Phone.Trim().Equals(loginRequest.Phone.Trim()) , include : u => u.Include(u => u.Role));
            if (user == null)
            {
                return null;
            }
            string Role = user.Role.Name;
            Tuple<string,Guid> guidClaim = new Tuple<string, Guid>("userId", user.Id);
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

        public async Task<bool> CheckoutNow(CheckoutRequest request)
        {
            var price = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(selector: x => x.Price, predicate: x => x.Id.Equals(request.ClassId));
            var user = await GetCurrentUser();
            var numberOfStudents = request.StudentsIdList?.Count();
            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id.Equals(request.ClassId),include : x => x.Include(x => x.Sessions));
            var session = cls.Sessions.FirstOrDefault(x => x.Id == x.Id);
            var classInstance = await _unitOfWork.GetRepository<ClassInstance>().GetListAsync(predicate: x => x.SessionId == session.Id);
            var numberOfStudentInClass = 0;
            if(classInstance != null)
            {
                numberOfStudentInClass = classInstance.Count();
            }
            var isExceed = numberOfStudentInClass + numberOfStudents > cls.LimitNumberStudent;
            if(isExceed)
            {
                throw new BadHttpRequestException($"you add exceed {numberOfStudents + numberOfStudents - cls.LimitNumberStudent} students at now ", StatusCodes.Status400BadRequest);
            }
            if (numberOfStudents == 0 || numberOfStudents == null)
            {
                throw new BadHttpRequestException("let add children", StatusCodes.Status400BadRequest);
            }
            if (user == null)
            {
                throw new BadHttpRequestException("user is unauthencated", StatusCodes.Status400BadRequest);
            }
            var transactionClassFee = new ClassFeeTransaction
            {
                ParentId = user.Id,
                Id = Guid.NewGuid(),
                ActualPrice = price * numberOfStudents,
                DateCreated = DateTime.Now,
            };
            await _unitOfWork.GetRepository<ClassFeeTransaction>().InsertAsync(transactionClassFee);
            bool isSuccessAtClassFee = await _unitOfWork.CommitAsync() > 0;
            if (!isSuccessAtClassFee)
            {
                throw new BadHttpRequestException("insert classfee is failed", StatusCodes.Status400BadRequest);
            }
            List<Guid> promotionList = request.UserPromotions;
            if (promotionList.Count > 0)
            {
                foreach (var promotionId in promotionList)
                {
                    var promtionTransaction = new PromotionTransaction
                    {
                        ClassFeeTransactionId = transactionClassFee.Id,
                        UserPromotionId = promotionId,
                        Id = Guid.NewGuid()
                    };
                    await _unitOfWork.GetRepository<PromotionTransaction>().InsertAsync(promtionTransaction);
                    bool isSuccessAtTransactionPromotion = await _unitOfWork.CommitAsync() > 0;
                    if (!isSuccessAtTransactionPromotion)
                    {
                        throw new BadHttpRequestException("insert transaction promotion is failed", StatusCodes.Status400BadRequest);
                    }
                }
            }
            List<Guid> StudentIds = request.StudentsIdList;
            foreach (var studentId in StudentIds)
            {
                var studentTransaction = new StudentTransaction
                {
                    Id = Guid.NewGuid(),
                    ClassTransactionId = transactionClassFee.Id,
                    StudentId = studentId,
                };
                await _unitOfWork.GetRepository<StudentTransaction>().InsertAsync(studentTransaction);
                bool isSuccessAtTransactionStudent = await _unitOfWork.CommitAsync() > 0;
                if (!isSuccessAtTransactionStudent)
                {
                    throw new BadHttpRequestException("insert transaction student is failed", StatusCodes.Status400BadRequest);
                }
            }
            var sessions = await _unitOfWork.GetRepository<Session>().GetListAsync(predicate: x => x.ClassId.Equals(request.ClassId));
            foreach (var sessionx in sessions)
            {
                foreach (var studentId in StudentIds)
                {
                    var classInstancex = new ClassInstance
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        Status = "UpComming",
                        StudentId = studentId,
                    };
                    await _unitOfWork.GetRepository<ClassInstance>().InsertAsync(classInstancex);
                    bool isSuccessAtClassInstance = await _unitOfWork.CommitAsync() > 0;
                    if (!isSuccessAtClassInstance)
                    {
                        throw new BadHttpRequestException("class Instance is failed", StatusCodes.Status400BadRequest);
                    }
                }
            }
            var actualPrice = price * numberOfStudents;
            var lastPrice = actualPrice;
            if(promotionList.Count > 0)
            {
                foreach(var promotion in promotionList)
                {
                    var userpromotion = await _unitOfWork.GetRepository<UserPromotion>().SingleOrDefaultAsync(predicate : x => x.Id.Equals(promotion),include : x => x.Include(x => x.Promotion));
                    var promotionx = userpromotion.Promotion;
                    var unit = promotionx.UnitDiscount;
                    if (unit.ToLower().Equals("cash"))
                    {
                        lastPrice = actualPrice - promotionx.DiscountValue;
                    } else
                    {
                        lastPrice = actualPrice * (1 - (promotionx.DiscountValue / 100));
                    }
                }
            }
            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>().SingleOrDefaultAsync(predicate: x => x.UserId.Equals(GetUserIdFromJwt()));
            if(personalWallet.Balance < lastPrice) 
            {
                throw new BadHttpRequestException("balance is not enough to payment", StatusCodes.Status400BadRequest);
            }
            personalWallet.Balance = personalWallet.Balance - lastPrice.Value;
            _unitOfWork.GetRepository<PersonalWallet>().UpdateAsync(personalWallet);
            bool isPersonalWallet = await _unitOfWork.CommitAsync() > 0;
            if (!isPersonalWallet)
            {
                throw new BadHttpRequestException("update failed at personal wallet", StatusCodes.Status400BadRequest);
            }

            return true;
        }

        public async Task<bool> CheckUserExistByPhone(string phone)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate : x => x.Phone.Trim().Equals(phone.Trim()));
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
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(predicate : x => x.Id == x.Id);
            return users.ToList();
        }

        public async Task<NewTokenResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            var userId = JwtUtil.ReadToken(refreshTokenRequest.OldToken);
            if(string.IsNullOrEmpty(userId)) {
                return null;
            }
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == Guid.Parse(userId), include: u => u.Include(u => u.Role));
            Tuple<string, Guid> guidClaim = new Tuple<string, Guid>("userId", user.Id);
            var token = JwtUtil.GenerateJwtToken(user, guidClaim);
            return new NewTokenResponse { Token = token };
        }

        public async Task<bool> RegisterNewUser(RegisterRequest registerRequest)
        {
            Address address = new Address
            {
                Street = registerRequest.Street,
                City = registerRequest.City,
                District = registerRequest.District,
                Id = Guid.NewGuid(),
            };
           await _unitOfWork.GetRepository<Address>().InsertAsync(address);
           var isAddressSuccess = await _unitOfWork.CommitAsync() > 0;
           if(!isAddressSuccess)
           {
                throw new BadHttpRequestException("address can't insert", StatusCodes.Status400BadRequest);
           }
           var role = await _unitOfWork.GetRepository<Role>().SingleOrDefaultAsync(predicate : x => x.Name.Equals(RoleEnum.PARENT.GetDescriptionFromEnum<RoleEnum>()),selector : x => x.Id);
           if(registerRequest.DateOfBirth > DateTime.Now)
            {
                throw new BadHttpRequestException("date of birth is previous now", StatusCodes.Status400BadRequest);
            }
            User user = new User
            {
                AddressId = address.Id,
                DateOfBirth = registerRequest.DateOfBirth,
                Email = registerRequest.Email,
                FullName = registerRequest.FullName,
                Gender = registerRequest.Gender,
                Phone = registerRequest.Phone,
                RoleId = role,
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
            return isSuccess;
        }
    }
}
