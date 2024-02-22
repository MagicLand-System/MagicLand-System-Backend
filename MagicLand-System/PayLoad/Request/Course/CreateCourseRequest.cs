namespace MagicLand_System.PayLoad.Request.Course
{
    public class CreateCourseRequest
    {
        public string CourseName {  get; set; } 
        public double Price {  get; set; }
        public int MinAge {  get; set; }    
        public int MaxAge { get; set; }
        public string MainDescription { get; set; } 
        public string Img {  get; set; }
        public string CourseCategoryId { get; set; }
        public string SyllabusId {  get; set; } 
        public List<SubDescriptionRequest>?  SubDescriptions { get; set; }   

    }
}
