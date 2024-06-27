using MagicLand_System_Web_Dev.Pages.Messages.InforMessage;

namespace MagicLand_System_Web_Dev.Pages.Messages.DefaultMessage
{
    public class CourseDefaultMessage : ApiInforMessage
    {
        public required string CourseName { get; set; }
        public required string SyllabusBelong { get; set; }
        public required double CoursePrice { get; set; }
        public required string AgeRange { get; set; }
    }
}
