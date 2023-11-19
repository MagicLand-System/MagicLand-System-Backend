namespace MagicLand_System.PayLoad.Response.Student
{
    public class StudentClassResponse
    {
        public string ClassName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string Method { get; set; }
        public string LecturerName { get; set; }
        public string StatusClass { get; set; }
        public string CourseName { get; set; }
        public int NumberOfSession { get; set; }
        public int MinYearOldsStudentOfCourse { get; set; }
        public int MaxYearOldsStudentOfCourse { get; set; }


    }
}
