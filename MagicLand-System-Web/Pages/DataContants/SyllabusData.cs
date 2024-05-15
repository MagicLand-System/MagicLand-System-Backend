using MagicLand_System_Web_Dev.Pages.Enums;
using System.Net.Http;
using System.Text;

namespace MagicLand_System_Web_Dev.Pages.DataContants
{
    public static class SyllabusData
    {
        public static readonly List<(string, string)> Subjects;
        public static readonly List<(string, string)> MusicSubjectNameData, LanguageSubjectNameData, MathSubjectNameData, DanceSubjectNameData,
                                            SingSubjectNameData, PhysicsSubjectNameData, ProgramerSubjectNameData, ArtSubjectNameData;

        public static readonly List<string> MusicDescription, LanguageDescription, MathDescription, DanceDescription,
                                            SingDescription, PhysicsDescription, ProgramerDescription, ArtDescription;

        public static readonly List<(string, string)> QuizType;

        static SyllabusData()
        {
            QuizType = new List<(string, string)>
            {
                ("Trắc nghiệm", "MUL"),  ("Ghép thẻ","FLA")
            };

            Subjects = new List<(string, string)>
            {
                ("Nhạc", "Music"), ("Ngôn Ngữ", "Language"), ("Toán", "Math"), ("Nhảy", "Dance"),
                ("Hát", "Sing"), ("Vật Lý", "Physics"), ("Lập Trình", "Programer"), ("Hội Họa", "Art")
            };

            MusicSubjectNameData = new List<(string, string)>
            {
                ("Học Nhạc Dân Tộc", "HNDT"), ("Học Nhạc Thiếu Nhi", "HNTN"), ("Nhạc Cơ Bản","NCB"), ("Nhạc Nân Cao", "NNC"), ("Học Nhạc Quanh Thế Giới", "HNQTG"),
                ("Thế Giới Giai Điệu", "TGGD"),("Tài Năng Âm Nhạc", "TNAN"), ("Thế Giới Âm Nhạc", "TGAN"), ("Nốt Nhạc Kỳ Diệu", "NNKD"), ("Khai Phá Âm Nhạc", "KPAN"),
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
                ("Lập Trình Scarth", "LTS"), ("Làm Quen Ngôn Ngữ Máy", "LQNNM"), ("Ngôn Ngữ Máy Cơ Bản", "NNMCB"), ("Ngôn Ngữ Máy Nân Cao", "NNMNC"), ("Học Lập Trình C++", "HLTC"),
                ("Học Lập Trình C++ Trung Cấp", "HLTCTC"), ("Lý Thuyết Lập Trình", "LTLT"), ("Thế Giới Ngôn Ngữ Máy", "TGNNM"), ("Lập Trình Cơ Bản", "LTCB"), ("Các Ngôn Ngữ Lập Trình", "CNNLT")
            };

            DanceSubjectNameData = new List<(string, string)>
            {
                ("Lập Trình Scarth", "LTS"), ("Làm Quen Ngôn Ngữ Máy", "LQNNM"), ("Ngôn Ngữ Máy Cơ Bản", "NNMCB"), ("Ngôn Ngữ Máy Nân Cao", "NNMNC"), ("Học Lập Trình C++", "HLTC"),
                ("Học Lập Trình C++ Trung Cấp", "HLTCTC"), ("Lý Thuyết Lập Trình", "LTLT"), ("Thế Giới Ngôn Ngữ Máy", "TGNNM"), ("Lập Trình Cơ Bản", "LTCB"), ("Các Ngôn Ngữ Lập Trình", "CNNLT")
            };

            SingSubjectNameData = new List<(string, string)>
            {
                ("Lập Trình Scarth", "LTS"), ("Làm Quen Ngôn Ngữ Máy", "LQNNM"), ("Ngôn Ngữ Máy Cơ Bản", "NNMCB"), ("Ngôn Ngữ Máy Nân Cao", "NNMNC"), ("Học Lập Trình C++", "HLTC"),
                ("Học Lập Trình C++ Trung Cấp", "HLTCTC"), ("Lý Thuyết Lập Trình", "LTLT"), ("Thế Giới Ngôn Ngữ Máy", "TGNNM"), ("Lập Trình Cơ Bản", "LTCB"), ("Các Ngôn Ngữ Lập Trình", "CNNLT")
            };

            PhysicsSubjectNameData = new List<(string, string)>
            {
                ("Lập Trình Scarth", "LTS"), ("Làm Quen Ngôn Ngữ Máy", "LQNNM"), ("Ngôn Ngữ Máy Cơ Bản", "NNMCB"), ("Ngôn Ngữ Máy Nân Cao", "NNMNC"), ("Học Lập Trình C++", "HLTC"),
                ("Học Lập Trình C++ Trung Cấp", "HLTCTC"), ("Lý Thuyết Lập Trình", "LTLT"), ("Thế Giới Ngôn Ngữ Máy", "TGNNM"), ("Lập Trình Cơ Bản", "LTCB"), ("Các Ngôn Ngữ Lập Trình", "CNNLT")
            };

            ArtSubjectNameData = new List<(string, string)>
            {
                ("Lập Trình Scarth", "LTS"), ("Làm Quen Ngôn Ngữ Máy", "LQNNM"), ("Ngôn Ngữ Máy Cơ Bản", "NNMCB"), ("Ngôn Ngữ Máy Nân Cao", "NNMNC"), ("Học Lập Trình C++", "HLTC"),
                ("Học Lập Trình C++ Trung Cấp", "HLTCTC"), ("Lý Thuyết Lập Trình", "LTLT"), ("Thế Giới Ngôn Ngữ Máy", "TGNNM"), ("Lập Trình Cơ Bản", "LTCB"), ("Các Ngôn Ngữ Lập Trình", "CNNLT")
            };

            MusicDescription = new List<string>
            {
                "Khơi dậy đam mê, khám phá thế giới âm nhạc, rèn luyện kỹ năng toàn diện và hơn cả một môn học. Tham gia ngay để đắm chìm trong thế giới âm nhạc đầy màu sắc và khơi dậy tiềm năng âm nhạc của bé!",
                "Được thiết kế bài bản, khoa học, phù hợp với mọi lứa tuổi và trình độ. Nội dung được chia thành các cấp độ từ cơ bản đến nâng cao, giúp bạn học tập một cách hiệu quả và dễ dàng tiếp thu kiến thức",
                "Bắt đầu một hành trình đầy màu sắc vào thế giới của âm nhạc. Từ những nốt nhạc đơn giản đến những bản hòa âm phức tạp, từ những giai điệu dân dã đến những tác phẩm kinh điển của các nhà soạn nhạc vĩ đại, bé sẽ cùng nhau tìm hiểu, khám phá và sáng tạo trong không gian âm nhạc",
                "Trên hành trình này, bé sẽ khám phá hấp dẫn của nghệ thuật âm nhạc qua các thế kỷ và văn hóa. Từ nền tảng cơ bản về âm nhạc đến những phong cách đa dạng và sự tiến bộ trong ngành,bé sẽ dành thời gian để tìm hiểu và trải nghiệm sức mạnh biểu cảm của âm nhạc. Dựa trên một cơ sở lý thuyết vững chắc và việc thực hành sáng tạo",
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

            DanceDescription = new List<string>
            {
                "Toán học không chỉ là một môn học, mà còn là một công cụ quan trọng để giúp trẻ phát triển tư duy logic, tăng cường khả năng giải quyết vấn đề và phát triển kỹ năng suy luận. Chương trình này được thiết kế để kích thích sự tò mò và ham muốn học của trẻ thông qua các hoạt động thú vị, trò chơi sáng tạo và bài học linh hoạt",
                "Cung cấp một môi trường học tập tích cực và động lực, giúp con bạn tiếp cận và hiểu biết về toán học một cách tự tin và hiệu quả nhất. Hãy cùng chúng tôi trải nghiệm niềm vui và ý nghĩa trong việc khám phá sức mạnh của con số và logic!",
                "Khóa học áp dùng phương pháp STEM, giúp trẻ phát huy tư duy sáng tạo cùng trí tưởng tượng trong môn toán. Chúng tôi hi vọng từ việc cảm nhận được toán học thật đẹp, hấp dẫn và gần gũi mà mỗi bé sẽ học toán bằng sự thích thú, từ đó phát triển tài năng của các em. Trong khóa học các bé được trải nghiệm các hoạt động được thiets kế khóe léo giúp các em tiếp cận toán học bằng sự quan sá, tưởng tượng, để phát triển trí thông minh, cảm xúc, phát triển năng lực giải quyết vấn đề và sáng tạo. Các bé sẽ được chơi mà học đầy cảm hứng.",
                "Cùng với sự phát triển nhanh chóng của thế giới hiện đại, việc có một nền tảng vững chắc về toán học là vô cùng quan trọng cho sự thành công của các em trong tương lai. Chương trình học Toán cho trẻ em từ 4-10 tuổi được thiết kế để giúp các em phát triển tư duy logic, kỹ năng sống và sự tự tin trong việc giải quyết vấn đề",
            };

            SingDescription = new List<string>
            {
                "Toán học không chỉ là một môn học, mà còn là một công cụ quan trọng để giúp trẻ phát triển tư duy logic, tăng cường khả năng giải quyết vấn đề và phát triển kỹ năng suy luận. Chương trình này được thiết kế để kích thích sự tò mò và ham muốn học của trẻ thông qua các hoạt động thú vị, trò chơi sáng tạo và bài học linh hoạt",
                "Cung cấp một môi trường học tập tích cực và động lực, giúp con bạn tiếp cận và hiểu biết về toán học một cách tự tin và hiệu quả nhất. Hãy cùng chúng tôi trải nghiệm niềm vui và ý nghĩa trong việc khám phá sức mạnh của con số và logic!",
                "Khóa học áp dùng phương pháp STEM, giúp trẻ phát huy tư duy sáng tạo cùng trí tưởng tượng trong môn toán. Chúng tôi hi vọng từ việc cảm nhận được toán học thật đẹp, hấp dẫn và gần gũi mà mỗi bé sẽ học toán bằng sự thích thú, từ đó phát triển tài năng của các em. Trong khóa học các bé được trải nghiệm các hoạt động được thiets kế khóe léo giúp các em tiếp cận toán học bằng sự quan sá, tưởng tượng, để phát triển trí thông minh, cảm xúc, phát triển năng lực giải quyết vấn đề và sáng tạo. Các bé sẽ được chơi mà học đầy cảm hứng.",
                "Cùng với sự phát triển nhanh chóng của thế giới hiện đại, việc có một nền tảng vững chắc về toán học là vô cùng quan trọng cho sự thành công của các em trong tương lai. Chương trình học Toán cho trẻ em từ 4-10 tuổi được thiết kế để giúp các em phát triển tư duy logic, kỹ năng sống và sự tự tin trong việc giải quyết vấn đề",
            };

            PhysicsDescription = new List<string>
            {
                "Toán học không chỉ là một môn học, mà còn là một công cụ quan trọng để giúp trẻ phát triển tư duy logic, tăng cường khả năng giải quyết vấn đề và phát triển kỹ năng suy luận. Chương trình này được thiết kế để kích thích sự tò mò và ham muốn học của trẻ thông qua các hoạt động thú vị, trò chơi sáng tạo và bài học linh hoạt",
                "Cung cấp một môi trường học tập tích cực và động lực, giúp con bạn tiếp cận và hiểu biết về toán học một cách tự tin và hiệu quả nhất. Hãy cùng chúng tôi trải nghiệm niềm vui và ý nghĩa trong việc khám phá sức mạnh của con số và logic!",
                "Khóa học áp dùng phương pháp STEM, giúp trẻ phát huy tư duy sáng tạo cùng trí tưởng tượng trong môn toán. Chúng tôi hi vọng từ việc cảm nhận được toán học thật đẹp, hấp dẫn và gần gũi mà mỗi bé sẽ học toán bằng sự thích thú, từ đó phát triển tài năng của các em. Trong khóa học các bé được trải nghiệm các hoạt động được thiets kế khóe léo giúp các em tiếp cận toán học bằng sự quan sá, tưởng tượng, để phát triển trí thông minh, cảm xúc, phát triển năng lực giải quyết vấn đề và sáng tạo. Các bé sẽ được chơi mà học đầy cảm hứng.",
                "Cùng với sự phát triển nhanh chóng của thế giới hiện đại, việc có một nền tảng vững chắc về toán học là vô cùng quan trọng cho sự thành công của các em trong tương lai. Chương trình học Toán cho trẻ em từ 4-10 tuổi được thiết kế để giúp các em phát triển tư duy logic, kỹ năng sống và sự tự tin trong việc giải quyết vấn đề",
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
                "Hiểu biết về lập trình không chỉ là một kỹ năng hữu ích mà còn là một công cụ mạnh mẽ để khuyến khích sự sáng tạo, logic và tư duy phê phán của trẻ. Chương trình này được thiết kế để giúp trẻ em tiếp cận lập trình một cách dễ dàng và thú vị thông qua các hoạt động tương tác, trò chơi sáng tạo và dự án thực hành. Hãy cùng nhau khám phá thế giới số hóa và phát triển kỹ năng lập trình cho bé!",
                "Cung cấp một môi trường học tập tích cực và động lực, giúp bé tiếp cận và hiểu biết về lập trình một cách tự tin và hiệu quả nhất. Hãy cùng chúng tôi trải nghiệm niềm vui và ý nghĩa trong việc khám phá sức mạnh của công nghệ và lập trình!",
                "Lập trình không chỉ là một kỹ năng, mà còn là một phương tiện để trẻ em khám phá và sáng tạo. Chính vì vậy, chương trình học Lập trình dành cho trẻ em từ 4-10 tuổi. Với phương pháp giảng dạy linh hoạt, đa dạng hoạt động và sự tập trung vào việc thúc đẩy sự sáng tạo và tư duy logic",
                "Cùng với sự phát triển của công nghệ, việc hiểu biết về lập trình trở thành một kỹ năng quan trọng cho tương lai của các bé. Chương trình học Lập trình cho trẻ em từ 4-10 tuổi được thiết kế để giúp các em phát triển kỹ năng sống và tư duy logic thông qua lập trình",
            };
        }

        public static string GetSyllabusDescription(string subjectCode)
        {
            Random random = new Random();

            switch (subjectCode.Trim())
            {
                case "Music":
                    return MusicDescription[random.Next(0, MusicDescription.Count)];
                case "Language":
                    return LanguageDescription[random.Next(0, LanguageDescription.Count)];
                case "Math":
                    return MathDescription[random.Next(0, MathDescription.Count)];
                case "Dance":
                    return DanceDescription[random.Next(0, DanceDescription.Count)];
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
                case "Music":
                    return MusicSubjectNameData[random.Next(0, MusicSubjectNameData.Count)];
                case "Language":
                    return LanguageSubjectNameData[random.Next(0, LanguageSubjectNameData.Count)];
                case "Math":
                    return MathSubjectNameData[random.Next(0, MathSubjectNameData.Count)];
                case "Dance":
                    return DanceSubjectNameData[random.Next(0, DanceSubjectNameData.Count)];
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
