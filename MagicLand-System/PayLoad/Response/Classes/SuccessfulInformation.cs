namespace MagicLand_System.PayLoad.Response.Classes
{
    public class SuccessfulInformation
    {
        public string ClassCode { get; set; }
        public string LecturerName { get; set; }
        public string RoomName { get; set; }
        public List<string> Times { get; set; }
        public DateTime? StartDate { get; set; }    
    }
}
