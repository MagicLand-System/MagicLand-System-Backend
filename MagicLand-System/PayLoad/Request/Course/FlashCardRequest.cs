using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Course
{
    public class FlashCardRequest
    {
        public string? RightSideImg { get; set; }
        public string? LeftSideImg { get; set; }
        [MaxLength(150, ErrorMessage = "Nội Dung Thẻ Không Nên Vượt Quá 150 Ký Tự")]
        [MinLength(1, ErrorMessage = "Nội Dung Thẻ Nên Có Ít Nhất 1 Ký Tự")]
        public string? RightSideDescription { get; set; }
        [MaxLength(150, ErrorMessage = "Nội Dung Thẻ Không Nên Vượt Quá 150 Ký Tự")]
        [MinLength(1, ErrorMessage = "Nội Dung Thẻ Nên Có Ít Nhất 1 Ký Tự")]
        public string? LeftSideDescription { get; set; }
        public double Score { get; set; } = 0;

    }
}
