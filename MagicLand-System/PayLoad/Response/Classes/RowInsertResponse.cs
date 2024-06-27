namespace MagicLand_System.PayLoad.Response.Classes
{
    public class RowInsertResponse
    {
        public int Index {  get; set; } 
        public bool IsSucess {  get; set; } 
        public string? Messsage {  get; set; } 
        public SuccessfulInformation? SuccessfulInformation { get; set; }
        public CreateClassResponse? CreateClass { get; set; }
    }
}
