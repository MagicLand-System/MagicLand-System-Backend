using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;

namespace MagicLand_System.Services.Implements
{
    public class DashBoardService : BaseService<DashBoardService>, IDashboardService
    {
        public DashBoardService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<DashBoardService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<DashboardRegisterResponse>> GetDashboardRegisterResponses(DateTime? startDate, DateTime? endDate)
        {
            var listStudentClass = await _unitOfWork.GetRepository<StudentClass>().GetListAsync(predicate : x => x.AddedTime >= startDate && x.AddedTime <= endDate);
            var listx = listStudentClass.GroupBy(x => x.AddedTime).ToList();
            List<DashboardRegisterResponse> responses = new List<DashboardRegisterResponse>();   
            foreach (var item in listx)
            {
                var count = item.Count();
                var date = item.Key;
                var day = date.Value.Day;
                var month = date.Value.Month;
                var response = new DashboardRegisterResponse
                {
                    NumberOfRegisters = count,
                    Date = day + "/" + month,
                };
                responses.Add(response);
            }
            return responses;   
        }
    }
}
