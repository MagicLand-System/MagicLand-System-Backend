namespace MagicLand_System_Web.Pages.DataContants
{
    public static class StudentData
    {
        public static readonly List<string> StudentMiddleNames;
        public static readonly List<string> StudentLastNames;
        public static readonly List<string> Genders;
        static StudentData()
        {
            StudentMiddleNames = new List<string>
            {
                "Trần", "Văn", "Quản", "Thị", "Bảo", "Gia", "Hùng", "Đức", "Trọng", "Việt", "Huyền", "Thanh", "Quốc", "Tiến"
            };

            StudentLastNames = new List<string>
            {
                "Ngọc", "Ngân", "Đạt", "Khanh", "Hoàng", "Trang", "Thư", "Anh", "My", "Trúc", "Mai", "Đào", "Ly", "Mỹ", "Hoàng"
            };

            Genders = new List<string>
            {
                "Nam", "Nữ"
            };
        }
    }
}
