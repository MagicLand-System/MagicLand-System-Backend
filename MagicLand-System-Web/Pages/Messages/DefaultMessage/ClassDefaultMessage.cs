using MagicLand_System_Web_Dev.Pages.Message.SubMessage;
using MagicLand_System_Web_Dev.Pages.Messages.InforMessage;

namespace MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage
{
    public class ClassDefaultMessage : ApiInforMessage
    {
        public string ClassId { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string CourseBeLong { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string LecturerBeLong { get; set; } = string.Empty;
        public string LecturerPhone { get; set; } = string.Empty;
        public int StudentRegistered { get; set; } = 0;
        public int MinStudentRegistered { get; set;} = 0;
        public int MaxStudentRegistered { get; set; } = 0;
        public List<ScheduleMessage> Schedules { get; set; } = default!;
    }
}
