using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Rooms;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.PayLoad.Response.Classes
{
    public class MyClassResponse : ClassResponse
    {
        public string? LecturerName { get; set; }
        public List<DailySchedule>? Schedules { get; set; }
        public string? CourseName { get; set; }
        public RoomResponse? RoomResponse { get; set; }
        public LecturerResponse? LecturerResponse { get; set; }
        public DateTime? CreatedDate { get; set; }
        public CustomCourseResponse? CourseResponse { get; set; }    
        public int? NumberOfStudentsRegister { get; set; }
        public int NumberOfClasses {  get; set; }
        public List<ClassScheduleResponse> ClassScheduleResponses { get; set; } = new List<ClassScheduleResponse>();
        public bool? CanChangeClass { get; set; }   
        public int? CurrentSession {  get; set; }   
        public string StudentStatus { get; set; }  

    }
}
