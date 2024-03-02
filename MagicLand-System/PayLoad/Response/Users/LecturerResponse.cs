namespace MagicLand_System.PayLoad.Response.Users
{
    public class LecturerResponse
    {
        public Guid LectureId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AvatarImage { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Role { get; set; }    
        public string LecturerField {  get; set; }  
        public int NumberOfClassesTeaching {  get; set; }   
    }
}
