namespace MagicLand_System.PayLoad.Response
{
    public class SessionResponse
    {
        public Guid Id { get; set; }
        public string? DayOfWeek { get; set; }
        public DateTime? Date { get; set; }
        
        public SlotResponse? Slot { get; set; }
        public RoomResponse? RoomResponse { get; set; }
    }
}
