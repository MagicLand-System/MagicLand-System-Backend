namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class OpeningScheduleResponse
    {
        public Guid? ClassId { get; set; } = default;
        public string? Schedule { get; set; } = "Unkow";
        public string? Slot { get; set; } = "Unkow";
        public DateTime? OpeningDay { get; set; } = default;
        public string? Method { get; set; } = "UnDefine";
    }

}
