using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Slots;

namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class ClassProgressResponse
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public SlotResponse Slot { get; set; } = new SlotResponse();
        public RoomResponse Room { get; set; } = new RoomResponse();
        public string? DayOfWeeks { get; set; }
    }
}
