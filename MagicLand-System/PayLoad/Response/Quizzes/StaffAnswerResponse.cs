namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class StaffAnswerResponse
    {
        public List<StaffMultipleChoiceResponse>? StaffMultiplechoiceAnswerResponses { get; set; } = null;
        public List<FlashCardAnswerResponseDefault>? FlashCardAnswerResponses { get; set; } = null;
    }
}
