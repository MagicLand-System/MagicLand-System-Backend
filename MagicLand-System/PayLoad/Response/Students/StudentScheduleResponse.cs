namespace MagicLand_System.PayLoad.Response.Students;
public class StudentScheduleResponse
{
    public required string StudentName { get; set; }
    public required string ClassCode { get; set; }
    public required string ClassName { get; set; }
    public required Guid ClassId { get; set; }
    public required Guid CourseId { get; set; }
    public required string ClassSubject { get; set; }
    public required string Address { get; set; }
    public required Guid TopicId { get; set; }
    public required Guid SessionId { get; set; }
    public string DayOfWeek { get; set; }
    public DateTime Date { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? RoomName { get; set; }
    public int? RoomInFloor { get; set; }
    public string? LinkURL { get; set; }
    public string? Method { get; set; }
    public string? AttendanceStatus { get; set; }
    public string? Note { get; set; }
    public string? LecturerName { get; set; }
    public int? EvaluateLevel { get; set; }
    public string? EvaluateDescription { get; set; }
    public string? EvaluateNote { get; set; }
    public string? Status { get; set; }

}
