using MagicLand_System_Web.Pages.Message.SubMessage;

namespace MagicLand_System_Web.Pages.Message
{
    public class ClassMessage
    {
        public string ClassCode { get; set; } = string.Empty;
        public string CourseBeLong { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string LecturerBeLong { get; set; } = string.Empty;
        public List<ClassSubMessage> Schedules { get; set; } = default!;
        public string Status { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
