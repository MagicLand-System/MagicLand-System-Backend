using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;

namespace MagicLand_System.Services.Interfaces
{
    public interface IRoomService
    {
        Task<List<Room>> GetRoomList(List<ScheduleRequest>? requests,DateTime? StartDate = null,string? CourseId = null);
    }
}
