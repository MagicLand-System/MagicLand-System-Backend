using MagicLand_System_Web.Pages.Messages.InforMessage;

namespace MagicLand_System_Web.Pages.Messages.DefaultMessage
{
    public class SyllabusDefaultMessage : ApiInforMessage
    {
        public string SyllabusName { get; set; } = string.Empty;
        public string SyllabusCode { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
    }
}
