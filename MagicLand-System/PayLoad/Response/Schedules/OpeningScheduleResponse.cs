namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class OpeningScheduleResponse
    {
        public Guid? ClassId { get; set; } = default;
        public DateTime? OpeningDay { get; set; } = default;
        public string? Schedule { get; set; } = "Undefine";
        public string? Slot { get; set; } = "Undefine";
        public string? Method { get; set; } = "UnDefine";
    }

}
