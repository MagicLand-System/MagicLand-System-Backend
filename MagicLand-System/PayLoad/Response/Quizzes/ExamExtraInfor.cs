using MagicLand_System.PayLoad.Response.Quizes;

namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class ExamExtraInfor : ExamResponse
    {
        public required Guid ClassId { get; set; }
        public string? ClassName { get; set; } = string.Empty;
        public string? Method { get; set; } = string.Empty;
        public string? RoomName { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;

    }
}
