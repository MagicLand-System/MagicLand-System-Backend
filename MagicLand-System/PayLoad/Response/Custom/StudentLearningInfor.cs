namespace MagicLand_System.PayLoad.Response.Custom
{
    public class StudentLearningInfor
    {
        public string StudentName { get; set; } = string.Empty;
        public List<AttendanceAndEvaluateInfor> LearningInfors { get; set; } = new List<AttendanceAndEvaluateInfor>();
    }
}
