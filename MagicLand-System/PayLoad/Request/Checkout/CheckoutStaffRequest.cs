using MagicLand_System.PayLoad.Request.Student;

namespace MagicLand_System.PayLoad.Request.Checkout
{
    public class CheckOutStaffRequest
    {
        public required RegisterRequest ParentInfor { get; set; }
        public required Guid ClassId { get; set; }
        public required List<CreateStudentRequest> StudentInfors { get; set; }
    }
}
