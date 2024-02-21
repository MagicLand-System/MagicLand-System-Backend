using MagicLand_System.Domain.Models;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class QuestionRequest
    {
        public string Description { get; set; }
        public string Img { get; set; }
        public List<MutipleChoiceAnswerRequest>? MutipleChoiceAnswerRequests { get; set; }
        public List<FlashCardRequest>? FlashCardRequests { get; set; } 
    }
}
