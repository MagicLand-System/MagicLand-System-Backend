using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Response;

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
        Task<bool> CheckoutNow(CheckoutRequest request);
    }
}
