namespace MagicLand_System.PayLoad.Response
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
