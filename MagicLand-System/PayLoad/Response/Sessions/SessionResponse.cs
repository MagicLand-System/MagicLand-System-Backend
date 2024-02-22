namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class SessionResponse
    {
        public int Order { get; set; }
        public List<SessionContentReponse>? Contents { get; set; } = new List<SessionContentReponse>();
    }
}
