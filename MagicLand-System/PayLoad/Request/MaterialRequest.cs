using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request
{
    public class MaterialRequest
    {
        public string? FileName {  get; set; }
        [Required(ErrorMessage = "Link Đường Dẫn Tài Nguyên Giáo Trình Đang Trống")]
        public required string URL { get; set; }
    }
}
