namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class SyllabusResponseV2
    {
        public Guid Id { get; set; }    
        public string SyllabusName {  get; set; }   
        public DateTime? EffectiveDate { get; set; }
        public string CourseName { get; set; } = "undefined";
        public string SubjectCode {  get; set; }    
        public DateTime? UpdateTime { get; set; }   
    }
}
