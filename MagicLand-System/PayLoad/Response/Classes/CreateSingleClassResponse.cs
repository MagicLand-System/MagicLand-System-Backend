namespace MagicLand_System.PayLoad.Response.Classes
{
    public class CreateSingleClassResponse
    {
        public bool Success { get; set; }
        public string ClassCode {  get; set; }  
        public string LecturerName { get; set; }
        public string RoomName {  get; set; }
        public List<ScheduleRequestV2> Times { get; set; } = new List<ScheduleRequestV2>();

    }
}
