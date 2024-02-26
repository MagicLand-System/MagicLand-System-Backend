namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class StaffQuestionResponse
    {
        public Guid QuestionId { get; set; }
        public string Description { get; set; }
        public string QuestionImg { get; set; }
        public StaffAnswerResponse StaffAnswerResponse { get; set; }    
    }
}
