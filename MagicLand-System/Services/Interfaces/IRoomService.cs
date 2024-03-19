using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.PayLoad.Response.Rooms;

namespace MagicLand_System.Services.Interfaces
{
    public interface IRoomService
    {
        Task<List<Room>> GetRoomList(FilterRoomRequest? request);
        Task<List<AdminRoomResponse>> GetAdminRoom(DateTime? startDate, DateTime? endDate,string? searchString,string? slotId);
        Task<List<AdminRoomResponseV2>> GetAdminRoomV2(DateTime date);

    }
}
