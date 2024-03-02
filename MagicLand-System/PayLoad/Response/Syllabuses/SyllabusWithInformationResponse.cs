namespace MagicLand_System.PayLoad.Response.Syllabuses
{
    public class SyllabusWithInformationResponse : SyllabusResponse
    {
        public SyllabusInforResponse? SyllabusInformations { get; set; } = new SyllabusInforResponse();
    }
}
