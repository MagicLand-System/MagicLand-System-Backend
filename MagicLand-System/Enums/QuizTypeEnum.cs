using System.ComponentModel;

namespace MagicLand_System.Enums
{
    public enum QuizTypeEnum
    {
        [Description("Nối Thẻ")]
        flashcard,
        [Description("Làm Tại Nhà")]
        offline,
        [Description("Làm Trên Máy")]
        online,
    }
}
