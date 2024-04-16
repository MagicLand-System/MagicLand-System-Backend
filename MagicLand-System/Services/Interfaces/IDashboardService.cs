using MagicLand_System.PayLoad;
using MagicLand_System.PayLoad.Response;

namespace MagicLand_System.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<List<DashboardRegisterResponse>> GetDashboardRegisterResponses(DateTime? startDate, DateTime? endDate);
        Task<NumberOfMemberResponse> GetOfMemberResponse();
        Task<List<FavoriteCourseResponse>> GetFavoriteCourseResponse(DateTime? startDate, DateTime? endDate);
        Task<List<RevenueDashBoardResponse>> GetRevenueDashBoardResponse(DateTime? startDate, DateTime? endDate);
    }
}
