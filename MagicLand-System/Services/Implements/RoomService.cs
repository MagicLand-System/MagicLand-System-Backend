using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;

namespace MagicLand_System.Services.Implements
{
    public class RoomService :  BaseService<RoomService>,IRoomService
    {
        public RoomService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<RoomService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<Room>> GetRoomList()
        {
            var rooms = await _unitOfWork.GetRepository<Room>().GetListAsync();
            if(rooms == null)
            {
                return null;
            } 
            return rooms.ToList();
        }
    }
}
