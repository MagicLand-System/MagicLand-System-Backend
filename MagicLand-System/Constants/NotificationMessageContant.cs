using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.Utils;

namespace MagicLand_System.Constants
{
    public class NotificationMessageContant
    {
        public const string ChangeClassRequestTitle = "Học Sinh Cần Chuyển Lớp";
        public const string ChangeClassTitle = "Học Sinh Được Chuyển Lớp";
        public const string MakeUpAttendanceTitle = "Học Sinh Cần Điểm Danh Bù";
        public const string MakeUpAttendanceLecturerTitle = "Nhắc Nhở Điểm Danh";
        public const string MakeUpEvaluateLecturerTitle = "Nhắc Nhở Đánh Giá";
        public const string PaymentSuccessTitle = "Đăng Ký Lớp Học Thành Công";
        public const string TopUpSuccessTitle = "Nạp Tiền Vào Ví Thành Công";
        public const string TopUpFailedTitle = "Nạp Tiền Vào Ví Không Thành Thành Công";
        public const string PaymentViaGatewaySuccessTitle = "Thanh Toán Đăng Ký Lớp Học Thành Công";
        public const string PaymentViaGatewayFailedTitle = "Thanh Toán Đăng Ký Lớp Học Không Thành Công";
        public const string RefundTitle = "Hoàn Tiền Từ Hệ Thống";
        public const string NoRefundTitle = "Không Hoàn Tiền Từ Hệ Thống";
        public const string LastDayRegisterTitle = "Hạn Cuối Đăng Ký";
        public const string RemindRegisterCourseTitle = "Nhanh Tay Đăng Ký Lớp Học";


        public static string ChangeClassRequestBody(string classCode, string studentName)
        {
            return $"Học Sinh {studentName} Thuộc Lớp {classCode} Cần Được Chuyển Lớp, Do Lớp Đã Hủy Vì Không Đủ Số Lượng Học Sinh";
        }
        public static string ChangeClassBody(string fromClass, string toClass, string studentName, ChangeClassReasoneEnum reason)
        {
            return $"Học Sinh {studentName} Đã Được Chuyển Từ Lớp {fromClass} Sang Lớp {toClass}, {EnumUtil.GetDescriptionFromEnum<ChangeClassReasoneEnum>(reason)}";
        }
        public static string MakeUpAttendanceBody(string classCode, string studentName, DateTime date)
        {
            return $"Học Sinh {studentName} Thuộc Lớp {classCode} Cần Được Điểm Danh Bù Vào Ngày {date}, Do Hệ Thống Không Nhận Thấy Trạng Thái Điểm Danh Của Bé";
        }
        public static string MakeUpAttendanceLecturerBody(Class cls, DateTime date, string slot)
        {
            return $"Bạn Có Lớp {cls.ClassCode} - {cls.Method}, {EnumUtil.CompareAndGetDescription<DayOfWeekEnum>(date.DayOfWeek.ToString())} {date} Vào Lúc {slot} Chưa Cập Nhập Điểm Danh. Vui Lòng Cập Nhập Điểm Danh";
        }
        public static string MakeUpEvaluateLecturerBody(Class cls, DateTime date, string slot)
        {
            return $"Bạn Có Một Số Học Sinh Ở Lớp {cls.ClassCode} - {cls.Method}, {EnumUtil.CompareAndGetDescription<DayOfWeekEnum>(date.DayOfWeek.ToString())} {date} Vào Lúc {slot} Chưa Cập Nhập Đánh Giá. Vui Lòng Cập Nhập Đánh Giá";
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

        public static string LastDayRegisterBody(string classCode)
        {
            return $"Hôm Nay Là Hạn Cuối Đăng Ký Lớp Học {classCode}, Hãy Nhanh Tay Đăng Ký Cho Bé Vào Lớp, Lớp Sẽ Xóa Khỏi Giỏ Hàng Khi Hết Hạn";
        }

        public static string RemindRegisterCourseBody(string courseName)
        {
            return $"Khóa Học {courseName} Vẫn Đang Có Các Lớp Đang Mở Và Còn Thời Hạn Đăng Ký, Hãy Nhanh Tay Đăng Ký Cho Bé Vào Lớp";
        }
    }
}
