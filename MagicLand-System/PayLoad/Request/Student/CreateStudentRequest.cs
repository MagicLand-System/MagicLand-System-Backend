using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Student
{
    public class CreateStudentRequest
    {
        [Required(ErrorMessage = "Full name is missing")]
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "Full name is missing")]
        public string Gender { get; set; }
        public string? AvatarImage { get; set; }
    }
}
