using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Services.Interfaces
{
    public interface ISyllabusService
    {
        Task<SyllabusResponse> GetSyllasbusResponse(string courseId);
        Task<List<SyllabusResponseV2>> GetAllSyllabus(string? keyword);
    }
}
