using MagicLand_System.PayLoad.Response.Students;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response
{
    public class StudentResultResponse
    {
        public StudentResponse StudentResponse { get; set; }
        public UserResponse Parent {  get; set; }   
    }
}
