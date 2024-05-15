using MagicLand_System_Web_Dev.Pages.Messages.SubMessage;

namespace MagicLand_System_Web_Dev.Pages.Messages.InforMessage
{
    public class StudentQuizInforMessage : ApiInforMessage
    {
        public string StudentName { get; set; } = string.Empty;
        public List<QuizMessage> Quizzes { get; set; } = new List<QuizMessage>();

    }
}
