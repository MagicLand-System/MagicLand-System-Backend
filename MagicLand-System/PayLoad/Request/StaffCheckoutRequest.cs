using MagicLand_System.PayLoad.Request.Checkout;
using MagicLand_System.PayLoad.Request.Student;

namespace MagicLand_System.PayLoad.Request
{
    public class StaffCheckoutRequest
    {
        public StaffUserCheckout StaffUserCheckout { get; set; }    
        public List<CheckoutRequest> Requests { get; set;}
        public List<CreateStudentRequest>? CreateStudentRequest { get; set; }    
        
    }
}
