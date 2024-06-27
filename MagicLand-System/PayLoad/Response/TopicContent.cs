using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.PayLoad.Response
{
    public class TopicContent
    {
        public int TopicIndex {  get; set; }
        public string TopicName { get; set; }
        public List<SessionContentReponse> Contents { get; set; } = new List<SessionContentReponse>();

    }
}
