using MagicLand_System.Enums;

namespace MagicLand_System.Constants
{
    public class NotificationMessageContant
    {
        public const string ChangeClassTitle = "Học Sinh Cần Chuyển Lớp";
        public const string MakeUpAttendanceTitle = "Học Sinh Cần Điểm Danh Bù";
        public const string PaymentSuccessTitle = "Đăng Ký Lớp Học Thành Công";
        public const string TopUpSuccessTitle = "Nạp Tiền Vào Ví Thành Công";
        public const string TopUpFailedTitle = "Nạp Tiền Vào Ví Không Thành Thành Công";
        public const string PaymentViaGatewaySuccessTitle = "Thanh Toán Đăng Ký Lớp Học Thành Công";
        public const string PaymentViaGatewayFailedTitle = "Thanh Toán Đăng Ký Lớp Học Không Thành Công";
        public const string RefundTitle = "Hoàn Tiền Từ Hệ Thống";
        public const string NoRefundTitle = "Không Hoàn Tiền Từ Hệ Thống";


        public static string ChangeClassBody(string classCode, string studentName)
        {
            return $"Học Sinh {studentName} Thuộc Lớp {classCode} Cần Được Chuyển Lớp, Do Lớp Đã Hủy Vì Không Đủ Số Lượng Học Sinh";
        }
        public static string MakeUpAttendanceBody(string classCode, string studentName, DateTime date)
        {
            return $"Học Sinh {studentName} Thuộc Lớp {classCode} Cần Được Điểm Danh Bù Vào Ngày {date}, Do Hệ Thống Không Nhận Thấy Trạng Thái Điểm Danh Của Bé";
        }
        public static string PaymentSuccessBody(string classCode, string studentName)
        {
            return $"Đăng Ký Thành Công Học Sinh {studentName} Vào Lớp {classCode} Lịch Điểm Danh Của Bé Sẽ Cập Nhập Khi Lớp Học Bắt Đầu";
        }
        public static string TopUpSuccessBody(string money, string bank)
        {
            return $"Nạp Thành Công Số Tiền {money} VND Vào Ví Từ Cổng Giao Dịch Qua Ngân Hàng {bank}";
        }
        public static string TopUpFailedBody(string money, string bank)
        {
            return $"Nạp Không Thành Công Số Tiền {money} VND Vào Ví Từ Cổng Giao Dịch Qua Ngân Hàng {bank}";
        }
        public static string PaymentViaGatewaySuccessBody(string classCode, string studentName, string bank)
        {
            return $"Thanh Toán Đăng Ký Lớp Học {classCode} Thành Công Cho Học Sinh {studentName}, Từ Cổng Giao Dịch Qua Ngân Hàng {bank}";
        }

        public static string PaymentViaGatewayFailedBody(string classCode, string studentName, string bank)
        {
            return $"Thanh Toán Đăng Ký Lớp Học {classCode} Không Thành Công Cho Học Sinh {studentName}, Từ Cổng Giao Dịch Qua Ngân Hàng {bank}";
        }
        public static string RefundBody(string classCode, string money, string studentName)
        {
            return $"Hệ Thống Đã Hoàn Trả {money} VND Vào Ví, Số Tiền Đăng Ký Lớp {classCode}, Do Học Sinh {studentName} Đã Hủy Đăng Ký Và Lớp Chưa Bắt Đầu";
        }
        public static string NoRefundBody(string classCode, string money, string studentName)
        {
            return $"Hệ Thống Sẽ Không Hoàn Trả {money} Số Tiền Đăng Ký Lớp {classCode} Của Học Sinh {studentName} Đã Hủy Đăng Ký, Do Lớp Đã Bắt Đầu Và Số Buổi Học Còn Lại Của Bé Cũng Bị Hủy";
        }
    }
}
