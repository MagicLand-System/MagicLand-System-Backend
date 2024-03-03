namespace MagicLand_System.PayLoad.Response.Slots
{
    public class SlotReponseForLecture
    {
        public Guid SlotId { get; set; } = default!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

    }
}
