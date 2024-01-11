namespace MagicLand_System.PayLoad.Response.Class
{
    public class StudentInClass
    {
        public Guid StudentId { get; set; }
        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string ParentName { get; set; }
        public string ParentPhoneNumber {  get; set; }  
    }
}
