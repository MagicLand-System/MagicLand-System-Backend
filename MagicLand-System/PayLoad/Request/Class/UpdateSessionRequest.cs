namespace MagicLand_System.PayLoad.Request.Class
{
    public class UpdateSessionRequest
    {
        public Guid? RoomId { get; set; }
        public Guid? LecturerId { get; set; }
        public Guid? SlotId { get; set;}
    }
}
