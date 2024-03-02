using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Slots;

namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class ScheduleWithoutLectureResponse
    {
        public Guid? ScheduleId { get; set; }
        public string? DayOfWeeks { get; set; }
        public DateTime Date { get; set; }

        public SlotResponse Slot { get; set; } = new SlotResponse();
        public RoomResponse Room { get; set; } = new RoomResponse();
    }
}
