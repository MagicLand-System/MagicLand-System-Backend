using MagicLand_System.PayLoad.Response.Room;
using MagicLand_System.PayLoad.Response.Slot;

namespace MagicLand_System.PayLoad.Response.Session
{
    public class SessionResponse
    {
        public Guid Id { get; set; }
        public int NoSession { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

    }
}
