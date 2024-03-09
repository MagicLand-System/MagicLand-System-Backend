using MagicLand_System.PayLoad.Response.Syllabuses.ForStaff;

namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class StaffSessionResponse
    {
        public Guid SessionId { get; set; }
        public int OrderSession { get; set; } = 1;
        public string TopicName { get; set; }
        public int OrderTopic { get; set; }
        public List<StaffSessionDescriptionResponse> Contents { get; set; }
        public StaffQuestionPackageResponse? StaffQuestionPackageResponse { get; set; }  = null;
      
    }
}
