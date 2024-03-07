namespace MagicLand_System.PayLoad.Response.Evaluates
{
    public class EvaluateResponse
    {
        public required int NoSession { get; set; }
        public required DateTime Date { get; set; }
        public required List<EvaluateStudentResponse> EvaludateInfors { get; set; }
    }
}
