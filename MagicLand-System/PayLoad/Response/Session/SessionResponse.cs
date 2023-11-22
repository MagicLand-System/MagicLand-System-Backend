using MagicLand_System.PayLoad.Response.Room;
using MagicLand_System.PayLoad.Response.Slot;

namespace MagicLand_System.PayLoad.Response.Session
{
    public class SessionResponse
    {
        public Guid Id { get; set; }
        public string? DayOfWeek { get; set; }
        public DateTime? Date { get; set; }

        public SlotResponse? Slot { get; set; }
        public RoomResponse? RoomResponse { get; set; }
    }
}
