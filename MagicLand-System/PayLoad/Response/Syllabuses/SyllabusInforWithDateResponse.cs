using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class SyllabusInforWithDateResponse
    {
        public List<SessionWithDateResponse>? Sessions { get; set; } = new List<SessionWithDateResponse>();
    }
}
