using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.PayLoad.Response.Sessions
{
    public class StaffSessionResponse
    {
        public Guid SessionId { get; set; }
        public int OrderSession { get; set; } = 1;
        public List<StaffSessionDescriptionResponse> Sessions { get; set; }
        public StaffQuestionPackageResponse? StaffQuestionPackageResponse { get; set; }  = null;
    }
}
