namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class ClassOpeningInfor
    {
        public Guid? ClassId { get; set; } = default;
        public DateTime? OpeningDay { get; set; } = default;
        public List<ScheduleShortenResponse> Schedules { get; set; } = new List<ScheduleShortenResponse>();
    }
}
