namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class SideFlashCardResponse
    {
        public  Guid SideFlashCardId { get; set; }
        public string SideFlashCardDescription { get; set; }
        public string SideFlashCardImage { get; set; }
        public string? Side { get; set; }
    }
}
