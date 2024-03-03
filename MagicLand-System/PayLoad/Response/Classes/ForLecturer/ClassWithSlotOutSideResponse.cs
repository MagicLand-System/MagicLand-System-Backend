using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;

namespace MagicLand_System.PayLoad.Response.Classes.ForLecturer
{
    public class ClassWithSlotOutSideResponse : ClassResponse
    {
        public string? SlotOrder { get; set; }
        public ScheduleCurrentLectureResponse? ScheduleInfors { get; set; } = new ScheduleCurrentLectureResponse(); 
    }
}
