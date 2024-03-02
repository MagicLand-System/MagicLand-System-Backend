using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Carts
{
    public class CartItemForClassResponse : CartItemResponse
    {
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required string Method { get; set; }
        public required int LimitNumberStudent { get; set; }
        public required int LeastNumberStudent { get; set; }
        public required string Video { get; set; }
        public required List<StudentResponse> Students { get; set; } = new List<StudentResponse>();
        public required UserResponse Lecture { get; set; }
        public required List<ScheduleResWithSession> Schedules { get; set; }
    }
}
