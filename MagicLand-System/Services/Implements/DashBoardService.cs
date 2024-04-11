using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using MagicLand_System.Utils;
using Microsoft.EntityFrameworkCore;

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

        public async Task<NumberOfMemberResponse> GetOfMemberResponse()
        {
            var users = await _unitOfWork.GetRepository<User>().GetListAsync(include: x => x.Include(x => x.Role));
            var parents = users.Where(x => x.Role.Name.Equals(RoleEnum.PARENT.GetDescriptionFromEnum<RoleEnum>()));
            var staff = users.Where(x => x.Role.Name.Equals(RoleEnum.STAFF.GetDescriptionFromEnum<RoleEnum>()) || x.Role.Name.Equals(RoleEnum.ADMIN.GetDescriptionFromEnum<RoleEnum>()) || x.Role.Name.Equals(RoleEnum.LECTURER.GetDescriptionFromEnum<RoleEnum>()));
            var students = await _unitOfWork.GetRepository<Student>().GetListAsync();   
            var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(predicate : x => x.Status.ToLower().Equals("upcoming"));
            return new NumberOfMemberResponse
            {
                NumOfChildrens = students.Count,
                NumOfCurrentClasses = classes.Count,
                NumOfParents = parents.Count(),
                NumOfStaffs = staff.Count(),
            };
        }
    }
}
