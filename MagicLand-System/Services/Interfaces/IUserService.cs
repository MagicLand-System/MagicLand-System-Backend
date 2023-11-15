using MagicLand_System.Domain.Models;

namespace MagicLand_System.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetUsers();
    }
}
