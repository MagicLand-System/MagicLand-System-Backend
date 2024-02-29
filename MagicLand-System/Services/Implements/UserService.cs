using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.User;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Schedules;
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

        public async Task<UserExistRespone> CheckUserExistByPhone(string phone)
        {
            var user = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Phone.Trim().Equals(phone.Trim()), include: x => x.Include(x => x.Role));
            if (user == null)
            {
                return new UserExistRespone
                {
                    IsExist = false,
                };
            }
            return new UserExistRespone
            {
                IsExist = true,
                Role = user.Role.Name,
            };
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
            catch (Exception ex)
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
                trans.CreateBy = request.FullName;
                trans.UpdateTime = DateTime.Now;
                trans.PersonalWalletId = personalWallet.Id;
                trans.PersonalWallet = personalWallet;
            }

            _unitOfWork.GetRepository<WalletTransaction>().UpdateRange(oldTransactions);
        }

        public async Task<List<LectureScheduleResponse>> GetLectureScheduleAsync()
        {
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate: x => x.LecturerId == GetUserIdFromJwt() && x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot!)
                .Include(x => x.Schedules).ThenInclude(sc => sc.Room!));

            if (!classes.Any())
            {
                throw new BadHttpRequestException("Giáo Viên Không Có Lịch Dạy Hoặc Lớp Học Chưa Bắt Đầu", StatusCodes.Status400BadRequest);
            }

            var responses = new List<LectureScheduleResponse>();
            foreach(var cls in classes)
            {
                responses.AddRange(ScheduleCustomMapper.fromClassToListLectureScheduleResponse(cls));
            }

            return responses;
        }
    }
}
