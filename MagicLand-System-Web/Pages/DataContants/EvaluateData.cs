using MagicLand_System.Enums;

namespace MagicLand_System_Web.Pages.DataContants
{
    public static class EvaluateData
    {
        public static readonly List<(string, string)> EvaluateNotes;
        public static readonly List<string> EvaluateQuizGood, EvaluateQuizNormal;
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

            EvaluateQuizGood = new List<string>
            {
                "Hiểu Bài Tốt",
                "Tiếp Tục Phát Huy",
                "Giữ Vững Phong Độ",
                "Bé Giỏi Qúa",
                "Bé Làm Tốt Lắm",
            };

            EvaluateQuizNormal = new List<string>
            {
                "Cần Cố Gắng Hơn",
                "Bé Hãy Nổ Lực hơn",
                "Bé Cần Chăm Chỉ Hơn Nhé",
                "Bé Đã Làm Tốt Rồi",
                "Tập Trung Hơn Nhé",
            };
        }

        public static string GetQuizEvaluate(int score, Random random)
        {
            if(score >= 5 && score <= 7)
            {
                return EvaluateQuizNormal[random.Next(0, EvaluateQuizNormal.Count)];
            }

            if(score > 7 && score <= 10)
            {
                return EvaluateQuizGood[random.Next(0, EvaluateQuizGood.Count)];
            }

            return string.Empty;
        }

    }
}
