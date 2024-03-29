using System.ComponentModel;

namespace MagicLand_System.Enums
{
    public enum PackageTypeEnum
    {
        [Description("Ôn Tập")]
        Review,
        [Description("Luyện Tập")]
        Practice,
        [Description("Tại Nhà")]
        ProgressTest,
        [Description("Tại Nhà")]
        Test,
        [Description("Điểm Danh")]
        Participation,
        [Description("Bài Tổng Kết")]
        FinalExam,
    }
}
