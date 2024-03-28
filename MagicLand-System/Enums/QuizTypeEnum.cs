using System.ComponentModel;

namespace MagicLand_System.Enums
{
    public enum QuizTypeEnum
    {
        [Description("Nối Thẻ")]
        FlashCard,
        [Description("Trắc Nghiệm")]
        MultipleChoice,
        [Description("Làm Tại Nhà")]
        Offline,
        [Description("Làm Trên Máy")]
        Online,
    }
}
