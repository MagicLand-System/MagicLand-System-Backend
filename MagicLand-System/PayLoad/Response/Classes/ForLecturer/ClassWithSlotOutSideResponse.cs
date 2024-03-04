using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;
using MagicLand_System.PayLoad.Response.Slots;

namespace MagicLand_System.PayLoad.Response.Classes.ForLecturer
{
    public class ClassWithSlotOutSideResponse : ClassResponse
    {
        public string? CourseName { get; set; }
        public string? SlotOrder { get; set; }
        public Guid? ScheduleId { get; set; }
        public string? DayOfWeeks { get; set; }
        public DateTime Date { get; set; }
        public SlotReponseForLecture Slot { get; set; } = new SlotReponseForLecture();
        public RoomResponse Room { get; set; } = new RoomResponse();
    }
}
