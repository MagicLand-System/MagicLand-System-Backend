using System.ComponentModel;

namespace MagicLand_System.Enums
{
    public enum ChangeClassReasoneEnum
    {
        [Description("")]
        NONE,
        [Description("Do Lớp Đã Hủy Vì Không Đủ Số Lượng Học Sinh")]
        CANCELED,
        [Description("Do Yêu Cầu Chuyển Lớp Từ Phụ Huynh")]
        REQUEST,

    }
}
