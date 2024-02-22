namespace MagicLand_System.PayLoad.Request.Course
{
    public class FlashCardRequest
    {
        public string RightSideImg {  get; set; }   
        public string LeftSideImg { get; set;}
        public string RightSideDescription {  get; set; }
        public string LeftSideDescription { get; set;}
        public double Score { get; set; }

    }
}
