﻿using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetUsers();
        Task<bool> CheckUserExistByPhone(string phone);
        Task<LoginResponse> Authentication(LoginRequest loginRequest);
        Task<User> GetCurrentUser();
        Task<NewTokenResponse> RefreshToken(RefreshTokenRequest refreshTokenRequest);
        Task<bool> RegisterNewUser(RegisterRequest registerRequest);
        Task<BillResponse> CheckoutAsync(List<CheckoutRequest> requests);
        Task<bool> ValidRegisterAsync(List<StudentScheduleResponse> schedules, Guid classId, List<Guid> studentIds);
        Task<List<LecturerResponse>> GetLecturers();
    }
}
