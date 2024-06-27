using MagicLand_System_Web_Dev.Pages.Messages.InforMessage;

namespace MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage
{
    public class StudentDefaultMessage : ApiInforMessage
    {
        public string StudentName { get; set; } = string.Empty;
        public string AccountArise { get; set; } = string.Empty;
        public string ParentBelong { get; set; } = string.Empty;
        public int Age { get; set; } = 0;
        public string Gender { get; set; } = string.Empty;

    }
}
