using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Full name is not missing")]
        [MaxLength(100, ErrorMessage = "Full name is max 100 chars")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Full name is not missing")]
        [MaxLength(20, ErrorMessage = "Full name is max 100 chars")]
        public string Phone { get; set; }
        [EmailAddress(ErrorMessage = "Invalid pattern email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Gender is missing")]
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; } = null;
        [Required(ErrorMessage = "Street is missing")]
   
        public string? Address {  get; set; }   

    }
}
