namespace MagicLand_System.PayLoad.Response.Rooms
{
    public class RoomResponse
    {
        public Guid RoomId { get; set; }
        public string? Name { get; set; }
        public int Floor { get; set; }
        public string? Status { get; set; }
        public string? LinkUrl { get; set; }
        public int Capacity { get; set; } = 100;
    }
}
