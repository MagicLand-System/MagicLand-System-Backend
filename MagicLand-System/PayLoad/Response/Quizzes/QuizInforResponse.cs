﻿namespace MagicLand_System.PayLoad.Response.Quizzes
{
    public class QuizInforResponse
    {
        public Guid ExamId { get; set; }
        public required string ExamName { get; set; }
        public required int ExamPart { get; set; }
        public required string QuizName { get; set; }

    }
}