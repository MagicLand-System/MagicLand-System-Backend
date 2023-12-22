using MagicLand_System.Domain.Models;

namespace MagicLand_System.Services.Interfaces
{
    public interface IRoomService
    {
        Task<List<Room>> GetRoomList();
    }
}
