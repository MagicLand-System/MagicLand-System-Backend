using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Slots;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class ScheduleResponse
    {
        public string? DayOfWeeks { get; set; }
        public DateTime Date { get; set; }
        
        public SlotResponse Slot { get; set; } = new SlotResponse();
        public RoomResponse Room { get; set; } = new RoomResponse();
    }
}
