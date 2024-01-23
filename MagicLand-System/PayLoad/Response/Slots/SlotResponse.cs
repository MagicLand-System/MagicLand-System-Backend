namespace MagicLand_System.PayLoad.Response.Slots
{
    public class SlotResponse
    {
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Guid SlotId { get; set; } = default!;
    }
}
