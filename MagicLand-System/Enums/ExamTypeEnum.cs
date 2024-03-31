using System.ComponentModel;

namespace MagicLand_System.Enums
{
    public enum ExamTypeEnum
    {
        [Description("Luyện Tập")]
        Practice,
        [Description("Kiểm Tra")]
        Test,
        [Description("Kiểm Tra Cuối Khóa")]
        FinalExam,
        [Description("Đã Hoàn Thành")]
        COMPLETED,
        [Description("Đã Hủy")]
        CANCELED,
    }
}
