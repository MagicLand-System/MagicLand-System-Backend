namespace MagicLand_System.PayLoad.Request.Evaluates
{
    public class EvaluateRequest
    {
        public required Guid ClassId { get; set; }
        public required  List<StudentEvaluateRequest> StudeEvaluateRequests { get; set; }
    }
}
