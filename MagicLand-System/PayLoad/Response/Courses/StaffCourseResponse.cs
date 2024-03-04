namespace MagicLand_System.PayLoad.Response.Courses
{
    public class StaffCourseResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? AddedDate { get; set; } = default;
        public string? SubjectName { get; set; }
        public int NumberOfSession { get; set; }
        public int? MinYearOldsStudent { get; set; } = 3;
        public int? MaxYearOldsStudent { get; set; } = 120;
        public string? Status { get; set; }
        public string? Image { get; set; } = null;
        public double Price { get; set; }
        public string? MainDescription { get; set; }
        public DateTime? UpdateDate { get; set; } = default;
        public Guid? SyllabusId { get; set; }
        public List<SubDescriptionTitleResponse> SubDescriptionTitles { get; set; }
    }
}
