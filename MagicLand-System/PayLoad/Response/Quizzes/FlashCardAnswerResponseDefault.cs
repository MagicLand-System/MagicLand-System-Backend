using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Quizzes.Staff;

namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class FlashCardAnswerResponseDefault
    {
        public Guid FlashCarId { get; set; }    
        public double Score {  get; set; }  
        public List<SideFlashCardResponse> SideFlashCardResponses { get; set; } 
    }
}
