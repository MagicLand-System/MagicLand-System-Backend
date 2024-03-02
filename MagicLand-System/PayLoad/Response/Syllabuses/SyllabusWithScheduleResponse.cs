using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class SyllabusWithScheduleResponse : SyllabusResponse
    {
        public SyllabusInforWithDateResponse? SyllabusInformations { get; set; } = new SyllabusInforWithDateResponse();
    }
}
