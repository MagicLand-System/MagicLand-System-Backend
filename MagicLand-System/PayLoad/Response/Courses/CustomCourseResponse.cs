namespace MagicLand_System.PayLoad.Response.Courses
{
    public class CustomCourseResponse
    {
        public string? Name { get; set; }
        public int? MinYearOldsStudent { get; set; } 
        public int? MaxYearOldsStudent { get; set; } 
        public string? Status { get; set; }
        public string? Image { get; set; } = null;
        public double Price { get; set; }
        public string? MainDescription { get; set; }
        public string SyllabusCode {  get; set; }  
        public string SyllabusName {  get; set; }   
        public string SyllabusType { get; set; }
        
    }
}
