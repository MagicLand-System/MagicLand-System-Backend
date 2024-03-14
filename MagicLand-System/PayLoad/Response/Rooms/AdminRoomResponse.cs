using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Rooms
{
    public class AdminRoomResponse
    {
        public string? Name { get; set; }
        public int? Floor { get; set; }
        public string? LinkURL { get; set; }
        public int Capacity { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public DateTime Date { get; set; }
        public string ClassCode { get; set; }   
    }
}
