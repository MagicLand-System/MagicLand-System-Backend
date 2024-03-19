namespace MagicLand_System.PayLoad.Response.Rooms
{
    public class AdminRoomResponseV2
    {
        public string? Name { get; set; }
        public int? Floor { get; set; }
        public string? LinkURL { get; set; }
        public int Capacity { get; set; }
        public List<RoomSchedule> Schedules { get; set; }
   
    }
}
