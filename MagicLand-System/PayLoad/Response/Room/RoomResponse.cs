namespace MagicLand_System.PayLoad.Response.Room
{
    public class RoomResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int Floor { get; set; }
        public string? Status { get; set; }
        public string? LinkUrl { get; set; }
    }
}
