using Org.BouncyCastle.Asn1.Mozilla;

namespace MagicLand_System.PayLoad.Response
{
    public class StudentGradeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }    
        public DateTime DateOfBirth {  get; set; }  
        public string Gender {  get; set; } 
        public string ParentPhoneNumber {  get; set; }  
        public string ImgAvatar {  get; set; }  
        public List<Transcript> Transcripts { get; set; }   = new List<Transcript>();
    }
}
