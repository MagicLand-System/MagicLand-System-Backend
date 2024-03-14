namespace MagicLand_System.PayLoad.Response.Users
{
    public class AdminLecturerResponse
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? AvatarImage { get; set; }
        public DateTime? DateOfBirth { get; set; } = null;
        public string? Address { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string LecturerField { get; set; }
        public string ClassCode { get; set; }
        public string ClassRoom { get; set; }   
    }
}
