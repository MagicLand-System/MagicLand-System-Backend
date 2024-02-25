namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class StaffQuestionPackageResponse
    {
        public Guid QuestionPackageId { get; set; }
        public string? Title { get; set; } = string.Empty;
        public string? Type { get; set; } = string.Empty;
        public int NoOfSession { get; set; } = 1;
        public int PackageOrder { get; set; }
        public int? Duration { get; set; }  
        public int? Deadline { get; set; }  
        public int? AttemptsAllowed { get; set; }   
        public int Score {  get; set; } 
    }
}
