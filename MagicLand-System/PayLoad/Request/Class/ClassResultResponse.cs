using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Classes;

namespace MagicLand_System.PayLoad.Request.Class
{
    public class ClassResultResponse
    {
        public int NumberOfClasses { get; set; }    
        public List<MyClassResponse> MyClassResponses { get; set; }
    }
}
