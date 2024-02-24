using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class SyllabusWithScheduleResponse : SyllabusResponse
    {
        public List<ScheduleResWithSession> Schedules { get; set; } = new List<ScheduleResWithSession>();
    }
}
