namespace MagicLand_System.PayLoad.Request.Course
{
    public class CoursePriceRequest
    {
        public Guid CourseId { get; set; }
        public double Price {  get; set; }
        public DateTime EffectiveDate { get; set; } 
    }
}
