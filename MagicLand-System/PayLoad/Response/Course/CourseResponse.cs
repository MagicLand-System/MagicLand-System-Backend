﻿using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Session;

namespace MagicLand_System.PayLoad.Response.Course
{
    public class CourseResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Subject { get; set; }
        public int? NumberOfSession { get; set; }
        public int? MinAgeStudent { get; set; }
        public int? MaxAgeStudent { get; set; }
        public string? Image { get; set; }
        public decimal? Price { get; set; }

        public List<CourseDescriptionResponse>? Description { get; set; }
        public List<CourseResponse>? CoursePrerequisites { get; set; }
        public List<SessionResponse>? Sessions { get; set; }
    }
}