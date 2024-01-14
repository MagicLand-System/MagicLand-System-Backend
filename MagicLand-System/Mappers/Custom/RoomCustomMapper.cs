using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Rooms;

namespace MagicLand_System.Mappers.Custom
{
    public class RoomCustomMapper
    {
        public static RoomResponse fromRoomToRoomResponse(Room room)
        {
            if (room == null)
            {
                return new RoomResponse();
            }

            RoomResponse response = new RoomResponse
            {
                RoomId = room.Id,
                Name = room.Name,
                Floor = room.Floor ?? 0,
                Status = room.Status!.ToString(),
                LinkUrl = room.LinkURL != null ? room.LinkURL : "NoLink",
                Capacity = room.Capacity,

            };

            return response;
        }
    }
}
