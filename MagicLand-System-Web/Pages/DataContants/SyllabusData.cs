namespace MagicLand_System_Web_Dev.Pages.DataContants
{
    public static class SyllabusData
    {
        public static readonly List<(string, string)> Subjects;
        public static readonly List<(string, string)> LanguageSubjectNameData, MathSubjectNameData, SingSubjectNameData, PhysicsSubjectNameData, ProgramerSubjectNameData, ArtSubjectNameData;

        public static readonly List<string> LanguageDescription, MathDescription, SingDescription, PhysicsDescription, ProgramerDescription, ArtDescription;

        public static readonly List<(string, string)> QuizType;

        static SyllabusData()
        {
            QuizType = new List<(string, string)>
            {
                ("Trắc nghiệm", "MUL"),  ("Ghép thẻ","FLA")
            };

            Subjects = new List<(string, string)>
            {
                 ("Ngôn Ngữ", "Language"), ("Toán", "Math"), ("Hát", "Sing"), ("Vật Lý", "Physics"), ("Lập Trình", "Programer"), ("Hội Họa", "Art")
            };

            LanguageSubjectNameData = new List<(string, string)>
            {
                ("Tiếng Anh Cơ Bản", "TACB"), ("Tiếng Anh Nân Cao", "TANC"), ("Tiếng Trung Cơ Sở", "TTCS"), ("Tiếng Trung Cải Tiến", "TTCT"), ("Tiếng Anh Giao Tiếp", "TAGT"),
                ("Tiếng Nhật Sơ Cấp", "TNSC"), ("Tiếng Nhật Cho Bé", "TNCB"), ("Bé Học Tiếng Anh", "BHTA"), ("Bé Học Tiếng Anh Cơ Sở", "BHTACS"), ("Tiếng Nhật Cho Bé", "TNCB")
            };

            MathSubjectNameData = new List<(string, string)>
            {
                ("Toán Tư Duy", "TTD"), ("Toán Tư Duy Cơ Bản", "TTDCB"), ("Toán Tư Duy Nân Cao", "TTDNC"), ("Toán Tiểu Học", "TTH"), ("Toán Tiểu Học Nân Cao", "TTHNC"),
                ("Toán Cho Bé", "TCB"), ("Thế Giới Hình Và Số", "TGHVS"), ("Hình Và Số Học", "HVSH"), ("Toán Là Ngôn Ngữ", "TLNN"), ("Tư Duy Giải Toán", "TDGT")
            };

            ProgramerSubjectNameData = new List<(string, string)>
            {
                ("Lập Trình", "LTS"), ("Làm Quen Ngôn Ngữ Máy", "LQNNM"), ("Ngôn Ngữ Máy Cơ Bản", "NNMCB"), ("Ngôn Ngữ Máy Nân Cao", "NNMNC"), ("Học Lập Trình C", "HLTC"),
                ("Học Lập Trình C Trung Cấp", "HLTCTC"), ("Lý Thuyết Lập Trình", "LTLT"), ("Thế Giới Ngôn Ngữ Máy", "TGNNM"), ("Lập Trình Cơ Bản", "LTCB"), ("Các Ngôn Ngữ Lập Trình", "CNNLT")
            };

            SingSubjectNameData = new List<(string, string)>
            {
                ("Thế Giới Âm Nhạc", "TGAN"), ("Gai Điệu Quanh Ta", "GDQT"), ("Bản Giao Hưởng Của Cuộc Sống", "NNMCB"), ("Nốt Nhạc Vui Tươi", "NNVT"), ("Bé Hoc Hát", "BHH"),
                ("Học Hát Cùng Nhau", "HHCN"), ("Lời Ca Cảm Xúc", "LCCX")
            };

            PhysicsSubjectNameData = new List<(string, string)>
            {
          ("Khám Phá Vật Lý", "KPVL"),
     ("Vật Lý Thú Vị", "VLTV"),
    ("Thí Nghiệm Vui", "TNV"),
    ("Vật Lý Quanh Ta", "VLQT"),
    ("Hiểu Về Ánh Sáng", "HVAS"),
    ("Vật Lý Học Cơ Bản", "VLHCB"),
    ("Chuyển Động Và Năng Lượng", "CDNL"),
    ("Cơ Học Đơn Giản", "CHDG")
};

            ArtSubjectNameData = new List<(string, string)>
{
    ("Vẽ Vui Nhộn", "VVN"),
    ("Màu Sắc Diệu Kỳ", "MSDK"),
    ("Khám Phá Hội Họa", "KPHH"),
    ("Vẽ Thế Giới Quanh Em", "VTGQE"),
    ("Nghệ Thuật Cắt Dán", "NTCD"),
    ("Học Vẽ Cơ Bản", "HVCB"),
    ("Tô Màu Hình Vẽ", "TMHV"),
    ("Vẽ Tranh Phong Cảnh", "VTP"),
    ("Vẽ Chân Dung", "VCD")
};




            LanguageDescription = new List<string>
            {
                "Trong thế giới ngày nay, việc sở hữu kỹ năng ngôn ngữ mạnh mẽ không chỉ là một ưu điểm mà còn là một yếu tố quan trọng trong sự phát triển của trẻ. Chương trình này được thiết kế để tạo điều kiện tối ưu cho sự học tập và phát triển ngôn ngữ của các bé, kết hợp giữa học thông qua trò chơi, hoạt động thực tế và sự tương tác xã hội",
                "Với chương trình học dành cho trẻ em từ 4-10 tuổi này, chúng tôi cam kết mang đến một môi trường học tập an toàn, sáng tạo và đầy kích thích. Chúng tôi tin rằng việc học ngôn ngữ không chỉ là việc nhận thức từ vựng và ngữ pháp, mà còn là việc khám phá và thể hiện bản thân. Hãy cùng chúng tôi tạo ra những trải nghiệm học tập đáng nhớ cho con bạn!",
                "Chương trình học ngôn ngữ cho trẻ em từ 4-10 tuổi. Với phương pháp giảng dạy linh hoạt, đa dạng hoạt động, và sự tận tâm của đội ngũ giáo viên, chúng tôi cam kết cung cấp một môi trường học tập tích cực và khuyến khích sự phát triển toàn diện của trẻ",
                "Kỹ năng giao tiếp và hiểu biết ngôn ngữ trở thành một phần không thể thiếu trong cuộc sống hàng ngày. Chương trình học ngôn ngữ cho trẻ em từ 4-10 tuổi của chúng tôi nhằm mục đích giúp các em xây dựng nền tảng vững chắc trong việc sử dụng và hiểu biết về ngôn ngữ",
            };

            MathDescription = new List<string>
            {
                "Toán học không chỉ là một môn học, mà còn là một công cụ quan trọng để giúp trẻ phát triển tư duy logic, tăng cường khả năng giải quyết vấn đề và phát triển kỹ năng suy luận. Chương trình này được thiết kế để kích thích sự tò mò và ham muốn học của trẻ thông qua các hoạt động thú vị, trò chơi sáng tạo và bài học linh hoạt",
                "Cung cấp một môi trường học tập tích cực và động lực, giúp con bạn tiếp cận và hiểu biết về toán học một cách tự tin và hiệu quả nhất. Hãy cùng chúng tôi trải nghiệm niềm vui và ý nghĩa trong việc khám phá sức mạnh của con số và logic!",
                "Khóa học áp dùng phương pháp STEM, giúp trẻ phát huy tư duy sáng tạo cùng trí tưởng tượng trong môn toán. Chúng tôi hi vọng từ việc cảm nhận được toán học thật đẹp, hấp dẫn và gần gũi mà mỗi bé sẽ học toán bằng sự thích thú, từ đó phát triển tài năng của các em. Trong khóa học các bé được trải nghiệm các hoạt động được thiets kế khóe léo giúp các em tiếp cận toán học bằng sự quan sá, tưởng tượng, để phát triển trí thông minh, cảm xúc, phát triển năng lực giải quyết vấn đề và sáng tạo. Các bé sẽ được chơi mà học đầy cảm hứng.",
                "Cùng với sự phát triển nhanh chóng của thế giới hiện đại, việc có một nền tảng vững chắc về toán học là vô cùng quan trọng cho sự thành công của các em trong tương lai. Chương trình học Toán cho trẻ em từ 4-10 tuổi được thiết kế để giúp các em phát triển tư duy logic, kỹ năng sống và sự tự tin trong việc giải quyết vấn đề",
            };

            SingDescription = new List<string>
{
    "Âm nhạc không chỉ là một hoạt động giải trí mà còn là một phương tiện quan trọng giúp trẻ phát triển kỹ năng ngôn ngữ, tự tin thể hiện bản thân và cảm nhận sâu sắc về thế giới xung quanh. Chương trình này được thiết kế để khơi dậy niềm đam mê âm nhạc của trẻ thông qua các hoạt động hát, trò chơi âm nhạc và bài học sáng tạo.",
    "Cung cấp một môi trường học tập vui vẻ và đầy động lực, giúp con bạn tiếp cận và yêu thích âm nhạc một cách tự nhiên và tự tin nhất. Hãy cùng trải nghiệm niềm vui và sự phấn khích khi khám phá thế giới âm thanh và giai điệu!",
    "Khóa học áp dụng phương pháp giáo dục âm nhạc hài hòa, giúp trẻ phát triển khả năng sáng tạo và trí tưởng tượng thông qua âm nhạc. Từ việc cảm nhận vẻ đẹp và sự cuốn hút của âm nhạc, mỗi bé sẽ yêu thích và học hát với niềm vui, từ đó phát triển tài năng của mình. Trong khóa học, các bé sẽ tham gia vào các hoạt động thiết kế khéo léo giúp các em tiếp cận âm nhạc thông qua quan sát, tưởng tượng và trải nghiệm, nhằm phát triển trí thông minh cảm xúc, năng lực giải quyết vấn đề và sáng tạo. Các bé sẽ được vui chơi và học tập một cách đầy cảm hứng.",
};


            PhysicsDescription = new List<string>
{
    "Vật lý không chỉ là một môn học khoa học mà còn là cánh cửa mở ra thế giới kỳ diệu xung quanh chúng ta. Chương trình này giúp trẻ khám phá các hiện tượng vật lý thông qua các thí nghiệm đơn giản nhưng đầy mê hoặc, từ đó nuôi dưỡng tình yêu khoa học và khám phá.",
    "Một môi trường học tập vui nhộn và đầy sáng tạo, nơi trẻ có thể tìm hiểu về các nguyên lý vật lý cơ bản như lực, chuyển động và năng lượng. Với sự hỗ trợ từ các giáo viên tận tâm, các bé sẽ tự tin và hào hứng hơn trong việc học tập và khám phá.",
    "Vật lý là nền tảng của nhiều công nghệ hiện đại, và việc hiểu biết về vật lý sẽ mở ra nhiều cơ hội cho trẻ trong tương lai. Khóa học của chúng tôi giúp trẻ xây dựng nền tảng kiến thức vững chắc, từ đó các em có thể tự tin bước vào những lĩnh vực khoa học và kỹ thuật sau này.",
    "Trong suốt khóa học, trẻ sẽ được khuyến khích tham gia vào các dự án nhóm, nơi các em có thể học cách làm việc cùng nhau để giải quyết các vấn đề và thực hiện các thí nghiệm. Đây là cơ hội tuyệt vời để phát triển kỹ năng làm việc nhóm và giao tiếp."
};


            ProgramerDescription = new List<string>
            {
                "Hiểu biết về lập trình không chỉ là một kỹ năng hữu ích mà còn là một công cụ mạnh mẽ để khuyến khích sự sáng tạo, logic và tư duy phê phán của trẻ. Chương trình này được thiết kế để giúp trẻ em tiếp cận lập trình một cách dễ dàng và thú vị thông qua các hoạt động tương tác, trò chơi sáng tạo và dự án thực hành. Hãy cùng nhau khám phá thế giới số hóa và phát triển kỹ năng lập trình cho bé!",
                "Cung cấp một môi trường học tập tích cực và động lực, giúp bé tiếp cận và hiểu biết về lập trình một cách tự tin và hiệu quả nhất. Hãy cùng chúng tôi trải nghiệm niềm vui và ý nghĩa trong việc khám phá sức mạnh của công nghệ và lập trình!",
                "Lập trình không chỉ là một kỹ năng, mà còn là một phương tiện để trẻ em khám phá và sáng tạo. Chính vì vậy, chương trình học Lập trình dành cho trẻ em từ 4-10 tuổi. Với phương pháp giảng dạy linh hoạt, đa dạng hoạt động và sự tập trung vào việc thúc đẩy sự sáng tạo và tư duy logic",
                "Cùng với sự phát triển của công nghệ, việc hiểu biết về lập trình trở thành một kỹ năng quan trọng cho tương lai của các bé. Chương trình học Lập trình cho trẻ em từ 4-10 tuổi được thiết kế để giúp các em phát triển kỹ năng sống và tư duy logic thông qua lập trình",
            };

            ArtDescription = new List<string>
{
    "Khóa học kết hợp giữa lý thuyết và thực hành, giúp trẻ hiểu rõ hơn về các nguyên lý nghệ thuật như bố cục, phối màu và hình học. Các em sẽ được thực hiện nhiều dự án sáng tạo, từ vẽ tranh phong cảnh đến làm thiệp thủ công.",
    "Với phương pháp giáo dục nghệ thuật tiên tiến, chúng tôi khuyến khích trẻ tự do khám phá và thử nghiệm với các chất liệu và kỹ thuật mới. Điều này không chỉ giúp trẻ phát triển kỹ năng vẽ mà còn kích thích trí tưởng tượng và khả năng sáng tạo.",
    "Chương trình học hội họa này được thiết kế để khơi dậy niềm đam mê nghệ thuật của trẻ. Các bài học sẽ giới thiệu về các danh họa nổi tiếng và phong cách nghệ thuật đa dạng, từ đó giúp trẻ mở rộng tầm nhìn và hiểu biết về nghệ thuật.",
            };

        }

        public static string GetSyllabusDescription(string subjectCode)
        {
            Random random = new Random();

            switch (subjectCode.Trim())
            {
                case "Language":
                    return LanguageDescription[random.Next(0, LanguageDescription.Count)];
                case "Math":
                    return MathDescription[random.Next(0, MathDescription.Count)];
                case "Sing":
                    return SingDescription[random.Next(0, SingDescription.Count)];
                case "Physics":
                    return PhysicsDescription[random.Next(0, PhysicsDescription.Count)];
                case "Programer":
                    return ProgramerDescription[random.Next(0, ProgramerDescription.Count)];
                case "Art":
                    return ArtDescription[random.Next(0, ArtDescription.Count)];
            }

            return string.Empty;
        }

        public static (string, string) GetSyllabusName(string subjectCode)
        {
            Random random = new Random();

            switch (subjectCode.Trim())
            {
                case "Language":
                    return LanguageSubjectNameData[random.Next(0, LanguageSubjectNameData.Count)];
                case "Math":
                    return MathSubjectNameData[random.Next(0, MathSubjectNameData.Count)];
                case "Sing":
                    return SingSubjectNameData[random.Next(0, SingSubjectNameData.Count)];
                case "Physics":
                    return PhysicsSubjectNameData[random.Next(0, PhysicsSubjectNameData.Count)];
                case "Programer":
                    return ProgramerSubjectNameData[random.Next(0, ProgramerSubjectNameData.Count)];
                case "Art":
                    return ArtSubjectNameData[random.Next(0, ArtSubjectNameData.Count)];
            }

            return (string.Empty, string.Empty);
        }
    }
}
