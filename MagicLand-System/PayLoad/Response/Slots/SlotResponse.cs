namespace MagicLand_System.PayLoad.Response.Slots
{
    public class SlotResponse
    {
        public string? SlotOrder { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Guid SlotId { get; set; } = default!;
    }
}
