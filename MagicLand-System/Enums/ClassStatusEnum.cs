using System.ComponentModel;

namespace MagicLand_System.Enums
{
    public enum ClassStatusEnum 
    {
        [Description("Sắp Diễn Ra")]
        DEFAULT,
        [Description("Sắp Diễn Ra")]
        UPCOMING,
        [Description("Đã Bắt Đầu")]
        PROGRESSING,
        [Description("Đã Hoàn Thành")]
        COMPLETED,
        [Description("Đã Hủy")]
        CANCELED,
    }
}
