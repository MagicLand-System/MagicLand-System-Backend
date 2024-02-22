using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Slots;

namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class SyllabusInforResponse
    {
        public List<SyllabusSessionResponse>? Sessions { get; set; } = new List<SyllabusSessionResponse>();
    }
}
