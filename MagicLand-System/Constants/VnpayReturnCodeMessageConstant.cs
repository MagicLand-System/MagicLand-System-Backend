namespace MagicLand_System.Constants
{
    public class VnpayReturnCodeMessageConstant
    {
        public const string OrderSuccess = "Giao Dịch Thành Công, Khách Hàng Có Thể Đóng Cổng Giao Dịch";
        public const string NoCodeResponse = "Giao Dịch Không Thành Công, Lỗi Phát Sinh Từ Cổng Giao Dịch Vui Lòng Chờ Đến Khi Hệ Thống Sử Lý";
        public const string InvalidSignature = "Giao Dịch Không Thành Công, Thông Tin Sai Chữ Ký Vui Lòng Thực Hiện Lại";
        public const string NotRegisterInternetBanking = "Giao Dịch Không Thành Công, Thẻ/Tài khoản Của Khách Hàng Chưa Đăng Ký Dịch Vụ InternetBanking";
        public const string ExceededTimeValidateInfor = "Giao Dịch Không Thành Công, Khách Hàng Xác Thực Thông Tin Thẻ/Tài Khoản Không Đúng Quá 3 Lần";
        public const string OrderTimeExpire = "Giao Dịch Không Thành Công, Đã Hết Hạn Chờ Thanh Toán. Xin Quý Khách Vui Lòng Thực Hiện Lại Giao Dịch.";
        public const string InconrrectOTP = "Giao Dịch Không Thành Công, Do Quý Khách Nhập Sai Mật Khẩu Xác Thực Giao Dịch (OTP)";
        public const string CancelOrder = "Giao Dịch Không Thành Công, Khách Hàng Hủy Giao Dịch";
        public const string ExceededOrderLimit = "Giao Dịch Không Thành Công, Tài Khoản Của Quý Khách Đã Vượt Quá Hạn Mức Giao Dịch Trong Ngày.";
        public const string SystemError = "Giao Dịch Không Thành Công, Lỗi Phát Sinh Từ Hệ Thống Vui Lòng Chờ Đến Khi Hệ Thống Sử Lý";

        public static Dictionary<int, string> GetCodeMessage = new Dictionary<int, string>
        {
            { -1, NoCodeResponse },
            { -2, SystemError },
            { -3, SystemError },
            { 0, OrderSuccess },
            { 9, NotRegisterInternetBanking},
            { 10, ExceededTimeValidateInfor},
            { 11, OrderTimeExpire},
            { 13, InconrrectOTP},
            { 24, CancelOrder },
            { 65, ExceededOrderLimit },
        };
    }
}
