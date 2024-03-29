﻿namespace MagicLand_System.PayLoad.Response.Students;
    public class StudentScheduleResponse
    {
        public required string StudentName { get; set; }
        public int DayOfWeek { get; set; }
        public DateTime Date { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? RoomName { get; set; }
        public int? RoomInFloor { get; set; }
        public string? LinkURL { get; set; }
        public string? Method { get; set; }
        public string? LecturerName { get; set; }
        public string? ClassName { get; set; }
        public string? Status { get; set; }

    }
