using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Request.User;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Bills;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetUsers();
        Task<UserExistRespone> CheckUserExistByPhone(string phone);
        Task<LoginResponse> Authentication(LoginRequest loginRequest);
        Task<User> GetCurrentUser();
        Task<NewTokenResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest);
        Task<bool> RegisterNewUser(RegisterRequest registerRequest);
        Task<List<LecturerResponse>> GetLecturers(FilterLecturerRequest? request);
        Task<UserResponse> UpdateUserAsync(UserRequest request);
        Task<List<LectureScheduleResponse>> GetLectureScheduleAsync();
    }
}
