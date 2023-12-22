using MagicLand_System.PayLoad.Response.Room;
using MagicLand_System.PayLoad.Response.Slot;

namespace MagicLand_System.PayLoad.Response.Schedule
{
    public class ScheduleResponse
    {
        public string? DayOfWeeks { get; set; }
        public DateTime Date { get; set; }
        
        public SlotResponse Slot { get; set; } = new SlotResponse();
        public RoomResponse Room { get; set; } = new RoomResponse();
    }
}
