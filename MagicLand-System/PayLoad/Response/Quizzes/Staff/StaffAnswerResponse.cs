namespace MagicLand_System.PayLoad.Response.Quizzes.Staff
{
    public class StaffAnswerResponse
    {
        public List<StaffMultipleChoiceResponse>? StaffMultiplechoiceAnswerResponses { get; set; } = null;
        public List<FlashCardAnswerResponseDefault>? FlashCardAnswerResponses { get; set; } = null;
    }
}
