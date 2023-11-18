namespace MagicLand_System.PayLoad.Request
{
    public class FilterCourseRequest
    {
        public string? KeyWord { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public int? NumberSession { get; set; }
    }
}
