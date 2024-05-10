using MagicLand_System.Enums;

namespace MagicLand_System_Web.Pages.DataContants
{
    public static class EvaluateData
    {
        public static readonly List<(string, string)> EvaluateNotes;
        static EvaluateData()
        {
            EvaluateNotes = new List<(string, string)>
            {
                (EvaluateStatusEnum.NORMAL.ToString(), "Bé Học Bình Thường"),
                (EvaluateStatusEnum.NORMAL.ToString(), "Bé Ngoan Ngoãn"),
                (EvaluateStatusEnum.NORMAL.ToString(), "Bé Học Tốt"),
                (EvaluateStatusEnum.NOTGOOD.ToString(), "Bé Cần Tập Trung Hơn"),
                (EvaluateStatusEnum.NOTGOOD.ToString(), "Cần Phải Cố Gắng"),
                (EvaluateStatusEnum.NOTGOOD.ToString(), "Bé Phải Nổ Lực Hơn"),
                (EvaluateStatusEnum.GOOD.ToString(), "Bé Học Rất Giỏi"),
                (EvaluateStatusEnum.GOOD.ToString(), "Bé Vô Cùng Ngoan Ngoãn"),
                (EvaluateStatusEnum.GOOD.ToString(), "Bé Làm Rất Tốt"),
            };
        }
    }
}
