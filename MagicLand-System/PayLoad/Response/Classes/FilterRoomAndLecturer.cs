using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class FilterRoomAndLecturer
    {
        public Room Room { get; set; }
        public LecturerResponse Lecturer { get; set;}
    }
}
