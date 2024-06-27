using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Evaluates
{
    public class StudentEvaluateRequest
    {
        public required Guid StudentId { get; set; }
        [Range(1, 3, ErrorMessage = "Mức Độ Đánh Giá Thuộc Từ 1 Đến 3")]
        public required int Level { get; set; }
        public string? Note { get; set; } = string.Empty;
    }
}
