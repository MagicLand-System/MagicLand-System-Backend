namespace MagicLand_System_Web_Dev.Pages.DataContants
{
    public static class StudentData
    {
        public static readonly List<string> StudentMiddleNames;
        public static readonly List<string> StudentLastNames;
        public static readonly List<string> Genders;
        public static readonly List<string> FemaleStudentImages, MaleStudentImages;
        static StudentData()
        {
            StudentMiddleNames = new List<string>
            {
                "Trần", "Văn", "Quản", "Thị", "Bảo", "Gia", "Hùng", "Đức", "Trọng", "Việt", "Huyền", "Thanh", "Quốc", "Tiến", "Phát", "Thủy", "Linh"
            };

            StudentLastNames = new List<string>
            {
                "Ngọc", "Ngân", "Đạt", "Khanh", "Hoàng", "Trang", "Thư", "Anh", "My", "Trúc", "Mai", "Đào", "Ly", "Mỹ", "Hoàng"
            };

            Genders = new List<string>
            {
                "Nam", "Nữ"
            };

            FemaleStudentImages = new List<string>
            {
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl1.jpg?alt=media&token=c45bd21c-770b-47e7-ba1d-ee846954aa73",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl2.jpg?alt=media&token=70a1c9e5-3502-46a2-b936-107c60fbfb1c",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl3.jpg?alt=media&token=14c5f3e0-7533-4d5a-b87c-5052ad56811e",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl4.jpg?alt=media&token=24010f2c-da57-4e81-b452-7470f26e6727",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl5.jpg?alt=media&token=f4dc4dbc-690f-4277-b161-cffed0163d74",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl6.jpg?alt=media&token=85ec290e-5d50-4491-b745-36b7dcd4ee57",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl7.jpg?alt=media&token=c63d96c9-c6a4-4856-bfe6-6f82ec96a465",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl8.jpg?alt=media&token=aa457de8-4ee8-433c-8cef-b29b6d0ad5fe",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl9.jpg?alt=media&token=e4a1f839-215b-46b7-859e-455351e688e9",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Girl%2Fgirl10.jpg?alt=media&token=86b2fdcb-d83c-42da-8f3d-9a9c7a9cacbd",
            };

            MaleStudentImages = new List<string>
            {
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Fboy1.jpg?alt=media&token=66331a31-4c62-4e35-bf31-dd80ed1a348c",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Flovepik-lively-and-cute-handsome-boy-and-child-image-png-image_401612082_wh860.png?alt=media&token=9a34d154-12d6-4fd4-8f47-8a497223c59c",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Fboy3.jpg?alt=media&token=f30fa6dc-dc0a-4bfa-9cf8-4bc06797ce76",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Fboy4.jpg?alt=media&token=db2ed139-097d-4260-84c6-6cb157d4e02d",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Fboy5.jpg?alt=media&token=3975a08e-55ca-4ad3-9e35-e434afc738dc",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Fboy6.jpg?alt=media&token=feb1a934-253a-4253-b351-c7c1f3217473",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Fboy7.jpg?alt=media&token=1c655373-7b94-4bac-8ac3-97fe23737831",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Fboy8.jpeg?alt=media&token=cc1732ee-f219-41fd-a64a-5018ea652b08",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Fboy9.jpg?alt=media&token=dff9d3ef-6946-4c60-8018-f4a448e52a82",
                "https://firebasestorage.googleapis.com/v0/b/magicland-capstone.appspot.com/o/Boy%2Fboy10.jpg?alt=media&token=71e9883f-adc4-4e06-ae21-b61a20616157",
            };
        }


        public static string GetStudentImage(string gender, Random random)
        {
            if (gender.ToLower() == "nam")
            {
                return MaleStudentImages[random.Next(1, MaleStudentImages.Count)];
            }
            else
            {
                return FemaleStudentImages[random.Next(1, FemaleStudentImages.Count)];
            }

            return string.Empty;
        }
    }
}
