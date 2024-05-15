namespace MagicLand_System_Web_Dev.Pages.DataContants
{
    public static class CourseData
    {
        public static readonly List<(string, string)> TeacherAndLearnStyleSubDescription, RangeAndPuporseSubDescription, ApproachAndLearnStyleSubDescription, BaseSubDescription,
            PracticalApplicationSubDescription, TeachAndMaterialSubDescription, EnviromentAndSocialSubDescription, EvaluateAndObservationSubDescripsion;
        public static readonly List<string> MainDescriptions;
        public static readonly List<(string, int)> TitleSubDescriptions;
        static CourseData()
        {

            TitleSubDescriptions = new List<(string, int)>
{
    ("Vai trò của giáo viên và phương pháp giảng dạy", 1),
    ("Phạm vi và mục tiêu của khóa học", 2),
    ("Cách tiếp cận và phương pháp học tập", 3),
    ("Cơ sở lý thuyết và kiến thức căn bản", 4),
    ("Ứng dụng thực tiễn và bài tập thực hành", 5),
    ("Công nghệ và tài nguyên học tập", 6),
    ("Môi trường học tập và cộng đồng hỗ trợ", 7),
    ("Phương pháp đánh giá và theo dõi tiến trình học tập", 8),
};

            EnviromentAndSocialSubDescription = new List<(string, string)>
{
    ("Môi trường học tập thân thiện", "Chúng tôi tạo ra một môi trường học tập thân thiện và thoải mái, nơi mọi học viên đều cảm thấy an toàn và được chào đón."),
    ("Xây dựng mối quan hệ", "Học viên sẽ có cơ hội xây dựng mối quan hệ và kết nối với nhau thông qua các hoạt động nhóm và sự hỗ trợ từ giáo viên và đồng học."),
    ("Hỗ trợ từ cộng đồng", "Chúng tôi khuyến khích học viên hỗ trợ và chia sẻ kiến thức với nhau thông qua các diễn đàn trực tuyến và nhóm thảo luận."),
    ("Khích lệ hợp tác", "Học viên sẽ được khuyến khích hợp tác với nhau trong quá trình học tập, từ việc chia sẻ ý kiến đến làm việc nhóm trong các dự án."),
    ("Giáo viên hỗ trợ", "Giáo viên sẽ luôn sẵn lòng hỗ trợ và tạo điều kiện cho sự phát triển của học viên, từ việc giải đáp thắc mắc đến hỗ trợ trong quá trình học tập."),
};

            EvaluateAndObservationSubDescripsion = new List<(string, string)>
{
    ("Phương pháp đánh giá linh hoạt", "Chúng tôi sử dụng các phương pháp đánh giá linh hoạt và đa dạng, từ bài kiểm tra đến dự án thực hành, để đảm bảo việc đánh giá phản ánh chính xác tiến trình học tập của mỗi học viên."),
    ("Theo dõi tiến trình", "Chúng tôi liên tục theo dõi và đánh giá tiến trình học tập của học viên để có thể điều chỉnh và cung cấp phản hồi phù hợp."),
    ("Phản hồi xây dựng", "Học viên sẽ nhận được phản hồi xây dựng và cụ thể từ giáo viên và đồng học để giúp họ hiểu rõ hơn về điểm mạnh và điểm yếu của mình."),
    ("Hỗ trợ từ giáo viên", "Giáo viên sẽ cung cấp hỗ trợ và hướng dẫn cụ thể cho học viên về cách cải thiện và phát triển từng khía cạnh trong quá trình học."),
    ("Tự đánh giá", "Học viên sẽ được khuyến khích tự đánh giá và đề xuất các phương pháp cải thiện và phát triển bản thân trong quá trình học."),
};


            ApproachAndLearnStyleSubDescription = new List<(string, string)>
{
    ("Học tập linh hoạt", "Chúng tôi áp dụng các phương pháp học tập linh hoạt và đa dạng, từ học tập độc lập đến học nhóm, từ bài giảng trực tuyến đến thảo luận trực tiếp, nhằm tạo ra một môi trường học tập phù hợp với mọi học viên."),
    ("Thảo luận và trao đổi", "Học viên sẽ được khuyến khích tham gia vào các hoạt động thảo luận, trao đổi ý kiến và chia sẻ kinh nghiệm, từ đó mở rộng kiến thức và hiểu biết của họ."),
    ("Hướng dẫn cá nhân", "Mỗi học viên sẽ nhận được sự hướng dẫn cá nhân từ giáo viên và các trợ giảng, giúp họ tiến bộ nhanh chóng và hiệu quả trong quá trình học tập."),
    ("Thực hành và áp dụng", "Chúng tôi khuyến khích học viên thực hành và áp dụng những kiến thức đã học thông qua các bài tập và dự án thực tế, từ đó củng cố và làm chắc chắn kiến thức."),
    ("Học qua ví dụ", "Chúng tôi sử dụng nhiều ví dụ và trường hợp thực tế để giúp học viên hiểu sâu hơn về cách áp dụng kiến thức vào các tình huống thực tế."),
};

            BaseSubDescription = new List<(string, string)>
{
    ("Kiến thức căn bản", "Khóa học này bắt đầu với những kiến thức cơ bản và lý thuyết căn bản, giúp học viên hiểu rõ hơn về nền tảng của chủ đề."),
    ("Làm chủ kiến thức", "Học viên sẽ được hướng dẫn cách hiểu và làm chủ các khái niệm và lý thuyết căn bản, từ đó phát triển và tiến xa hơn trong học tập."),
    ("Lập trình và thực hành", "Học viên sẽ tham gia vào các bài tập lập trình và thực hành, giúp họ nắm vững các khái niệm và kỹ thuật cơ bản."),
    ("Tạo kiến thức sâu", "Chúng tôi tập trung vào việc giúp học viên xây dựng kiến thức sâu và lâu dài, từ những kiến thức căn bản đến những phát triển tiên tiến hơn."),
    ("Tự học và nghiên cứu", "Học viên sẽ được khuyến khích tự học và nghiên cứu thêm về chủ đề thông qua việc tìm hiểu và thực hành thêm ngoài lớp học."),
};

            PracticalApplicationSubDescription = new List<(string, string)>
{
    ("Ứng dụng thực tiễn", "Khóa học này không chỉ dừng lại ở việc truyền đạt kiến thức mà còn tạo cơ hội cho học viên thực hành và áp dụng những kiến thức đã học vào các tình huống thực tế."),
    ("Bài tập thực hành", "Học viên sẽ được tham gia vào các bài tập thực hành, từ những bài tập đơn giản đến những dự án phức tạp, giúp họ áp dụng kiến thức vào thực tế và rèn luyện kỹ năng."),
    ("Sử dụng công cụ thực tế", "Chúng tôi sẽ giới thiệu và hướng dẫn học viên sử dụng các công cụ và phần mềm thực tế trong lĩnh vực cụ thể, từ đó họ có thể áp dụng kiến thức vào công việc thực tế."),
};

            TeachAndMaterialSubDescription = new List<(string, string)>
{
    ("Công nghệ tiên tiến", "Chúng tôi sử dụng công nghệ tiên tiến và các tài nguyên học tập hiện đại nhất để hỗ trợ quá trình học tập của học viên."),
    ("Tài liệu đa dạng", "Học viên sẽ tiếp cận các tài liệu học tập đa dạng và phong phú, từ sách giáo khoa đến tài liệu trực tuyến và video giảng dạy."),
    ("Thực hành thêm", "Chúng tôi cung cấp các bài tập thực hành bổ sung và các tài liệu tham khảo để học viên có thể tiếp tục thực hành và nâng cao kỹ năng sau khi kết thúc khóa học."),
};


            TeacherAndLearnStyleSubDescription = new List<(string, string)>
{
    ("Khám phá kỹ năng", "Tạo cơ hội cho việc khám phá và phát triển kỹ năng mới mẻ."),
    ("Lý thuyết và thực tiễn đang xen", "Học cách áp dụng kiến thức lý thuyết vào thực tiễn thông qua các bài tập thực hành."),
    ("Giảng viên là cốt lỏi", "Giảng viên giàu kinh nghiệm và nhiệt huyết sẽ hướng dẫn bạn qua từng bước của khóa học."),
    ("Phương pháp mới, hiểu quả rõ ràng", "Phương pháp dạy học linh hoạt và cá nhân hóa, phù hợp với mọi trình độ học viên."),
    ("Phương pháp hiện đại", "Khám phá các phương tiện học tập đa dạng và hiện đại nhất."),
    ("Tương tác xã hội", "Xây dựng mối quan hệ xã hội và mạng lưới liên kết trong ngành thông qua khóa học này."),
    ("Áp dụng kiến thức vào thực tiễn", "Học viên sẽ được thực hành áp dụng những kiến thức họ học được vào các bài tập và dự án thực tế."),
};


            RangeAndPuporseSubDescription = new List<(string, string)>
{
    ("Đa dạng và phong phú", "Khóa học này bao gồm một loạt các chủ đề và nội dung, từ những khái niệm cơ bản đến những ứng dụng phức tạp, nhằm phục vụ nhu cầu học tập của mọi học viên."),
    ("Mục tiêu cụ thể", "Khóa học này đặt ra những mục tiêu rõ ràng và cụ thể, giúp học viên hiểu được những gì họ sẽ học và đạt được sau khi hoàn thành khóa học."),
    ("Ứng dụng trong thực tế", "Mục tiêu của khóa học là không chỉ cung cấp kiến thức lý thuyết mà còn tạo ra cơ hội cho học viên áp dụng những kiến thức đã học vào các tình huống thực tế."),
    ("Phù hợp với mọi đối tượng", "Khóa học này phù hợp với mọi đối tượng học viên, từ người mới bắt đầu đến những người đã có kiến thức sẵn có trong lĩnh vực."),
    ("Chuyên sâu và toàn diện", "Khóa học này không chỉ tập trung vào một phần nhỏ của chủ đề mà còn khám phá sâu hơn vào các khía cạnh và ứng dụng khác nhau của nó."),
};



            MainDescriptions = new List<string>
{
    "Được thiết kế bởi một đội ngũ giáo viên chất lượng và giàu kinh nghiệm, khóa học này không chỉ đảm bảo việc truyền đạt kiến thức một cách chuyên sâu mà còn tạo điều kiện cho sự phát triển toàn diện của học viên. Đội ngũ giáo viên của chúng tôi không chỉ là những chuyên gia trong lĩnh vực mà còn là những người có tâm huyết và tận tụy trong việc giáo dục.",
    "Khóa học này mang lại một cơ hội học tập sâu rộng và đa chiều trong lĩnh vực cụ thể, từ những khái niệm cơ bản đến những ứng dụng phức tạp. Học viên sẽ được khám phá những khía cạnh mới mẻ và thú vị của chủ đề, từ đó mở rộng kiến thức và hiểu biết của họ về lĩnh vực này.",
    "Chương trình học tập không chỉ tập trung vào việc truyền đạt kiến thức mà còn đặt mục tiêu phát triển kỹ năng cụ thể và ứng dụng trong thực tiễn. Học viên sẽ được tham gia vào các hoạt động thực hành, dự án, và bài tập giúp họ áp dụng những kiến thức đã học vào các tình huống thực tế.",
    "Khóa học cam kết tạo ra một môi trường học tập tích cực và động viên sự tiến bộ của mỗi học viên. Không chỉ là nơi học, mà chúng tôi còn xem trường lớp như một cộng đồng hỗ trợ, nơi mọi người cùng chia sẻ, hỗ trợ và khích lệ lẫn nhau.",
    "Khóa học này sử dụng các phương tiện học tập hiện đại và phong phú nhất để hỗ trợ quá trình học. Từ tài liệu học tập đến các bài giảng trực tuyến và các công cụ tương tác, chúng tôi đảm bảo rằng học viên có mọi thứ cần thiết để tiếp cận và tiêu thụ thông tin một cách hiệu quả.",
    "Khóa học đặt mục tiêu tạo ra những trải nghiệm học tập tương tác và thú vị thông qua các hoạt động nhóm và bài tập. Học viên sẽ có cơ hội làm việc cùng nhau, thảo luận và giải quyết vấn đề, từ đó học hỏi và phát triển từ kinh nghiệm của nhau.",
    "Khóa học tận dụng mọi cơ hội để xây dựng nền tảng kiến thức vững chắc và phát triển khả năng tự học và nghiên cứu của học viên. Điều này không chỉ giúp họ hiểu sâu hơn về lĩnh vực mà còn giúp họ trở thành những học viên tự tin và độc lập.",
    "Hướng dẫn từng bước một trong việc hiểu và áp dụng kiến thức vào các tình huống thực tế. Giáo viên sẽ không chỉ đơn thuần truyền đạt kiến thức mà còn hướng dẫn học viên áp dụng những kiến thức đã học vào những bài tập và dự án cụ thể.",
    "Một trong những mục tiêu chính của chúng tôi là tạo ra một môi trường học tập hỗ trợ và thân thiện, khuyến khích sự chia sẻ và hợp tác giữa học viên. Chúng tôi tin rằng việc học cùng nhau và hỗ trợ lẫn nhau là chìa khóa để đạt được thành công.",
    "Hỗ trợ học viên phát triển kỹ năng giao tiếp, tư duy logic, và giải quyết vấn đề. Không chỉ giáo dục về kiến thức, mà chúng tôi còn đặc biệt quan tâm đến việc phát triển kỹ năng mềm và sự chuẩn bị cho cuộc sống sau này của học viên."
};

        }

        public static ((string, string), int) GetSubDescription(int number, Random random, List<int> storedIndex)
        {
            int randomIndex = random.Next(0, 3);

            switch (number)
            {
                case 1:
                    if (storedIndex.Contains(randomIndex))
                    {
                        do
                        {
                            randomIndex = random.Next(0, TeacherAndLearnStyleSubDescription.Count);
                        } while (storedIndex.Contains(randomIndex));
                    }

                    return (TeacherAndLearnStyleSubDescription[randomIndex], randomIndex);
                case 2:
                    if (storedIndex.Contains(randomIndex))
                    {
                        do
                        {
                            randomIndex = random.Next(0, RangeAndPuporseSubDescription.Count);
                        } while (storedIndex.Contains(randomIndex));
                    }
                    return (RangeAndPuporseSubDescription[randomIndex], randomIndex);
                case 3:
                    if (storedIndex.Contains(randomIndex))
                    {
                        do
                        {
                            randomIndex = random.Next(0, ApproachAndLearnStyleSubDescription.Count);
                        } while (storedIndex.Contains(randomIndex));
                    }
                    return (ApproachAndLearnStyleSubDescription[randomIndex], randomIndex);
                case 4:
                    if (storedIndex.Contains(randomIndex))
                    {
                        do
                        {
                            randomIndex = random.Next(0, BaseSubDescription.Count);
                        } while (storedIndex.Contains(randomIndex));
                    }
                    return (BaseSubDescription[randomIndex], randomIndex);
                case 5:
                    if (storedIndex.Contains(randomIndex))
                    {
                        do
                        {
                            randomIndex = random.Next(0, PracticalApplicationSubDescription.Count);
                        } while (storedIndex.Contains(randomIndex));
                    }
                    return (PracticalApplicationSubDescription[randomIndex], randomIndex);
                case 6:
                    if (storedIndex.Contains(randomIndex))
                    {
                        do
                        {
                            randomIndex = random.Next(0, TeachAndMaterialSubDescription.Count);
                        } while (storedIndex.Contains(randomIndex));
                    }
                    return (TeachAndMaterialSubDescription[randomIndex], randomIndex);
                case 7:
                    if (storedIndex.Contains(randomIndex))
                    {
                        do
                        {
                            randomIndex = random.Next(0, EnviromentAndSocialSubDescription.Count);
                        } while (storedIndex.Contains(randomIndex));
                    }
                    return (EnviromentAndSocialSubDescription[randomIndex], randomIndex);
                case 8:
                    if (storedIndex.Contains(randomIndex))
                    {
                        do
                        {
                            randomIndex = random.Next(0, EvaluateAndObservationSubDescripsion.Count);
                        } while (storedIndex.Contains(randomIndex));
                    }
                    return (EvaluateAndObservationSubDescripsion[randomIndex], randomIndex);
            }

            return ((string.Empty, string.Empty), 0);
        }
    }
}
