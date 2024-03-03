using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Slots;

namespace MagicLand_System.PayLoad.Response.Schedules.ForLecturer
{
    public class ScheduleCurrentLectureResponse
    {
        public Guid? ScheduleId { get; set; }
        public string? DayOfWeeks { get; set; }
        public DateTime Date { get; set; }

        public SlotReponseForLecture Slot { get; set; } = new SlotReponseForLecture();
        public RoomResponse Room { get; set; } = new RoomResponse();
    }

}
