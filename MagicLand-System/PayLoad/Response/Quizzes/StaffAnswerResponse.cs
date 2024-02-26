namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class StaffAnswerResponse
    {
        public List<StaffMultipleChoiceResponse>? StaffMultiplechoiceAnswerResponses { get; set; } = null;
        public List<FlashCardAnswerResponse>? FlashCardAnswerResponses { get; set; } = null;
    }
}
