namespace MagicLand_System.PayLoad.Response.Classes
{
    public class InsertClassesResponse
    {
        public int SuccessRow {  get; set; }
        public int FailureRow { get; set; }
        public List<RowInsertResponse> RowInsertResponse { get; set; }
    }
}
