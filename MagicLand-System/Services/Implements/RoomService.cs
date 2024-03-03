using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Request;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using System;

namespace MagicLand_System.Services.Implements
{
    public class RoomService :  BaseService<RoomService>,IRoomService
    {
        public RoomService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<RoomService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<Room>> GetRoomList(List<ScheduleRequest>? requests = null,DateTime? startDate = null , string? courseId = null)
        {
            var rooms = await _unitOfWork.GetRepository<Room>().GetListAsync();
            if(rooms == null)
            {
                return null;
            } 
            if(requests != null && startDate != null && courseId != null)
            {
                var numberOfSession = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate : x => x.Id.ToString().Equals(courseId),selector : x => x.NumberOfSession);
                var weeks = numberOfSession / (requests.Count);
                var endDate = startDate.Value.AddDays(7 * weeks);
                List<Room> excepted = new List<Room>();
                foreach(var request in requests) 
                {
                    if (request.DateOfWeek.ToLower().Equals("sunday"))
                    {
                        
                    }
                    if (request.DateOfWeek.ToLower().Equals("monday"))
                    {
                    }
                    if (request.DateOfWeek.ToLower().Equals("tuesday"))
                    {
                    }
                    if (request.DateOfWeek.ToLower().Equals("wednesday"))
                    {
                    }
                    if(request.DateOfWeek.ToLower().Equals("thursday"))
                    {
                    }
                    if (request.DateOfWeek.ToLower().Equals("friday"))
                    {
                    }
                    if (request.DateOfWeek.ToLower().Equals("saturday"))
                    {
                    }
                }
            }
            return rooms.OrderBy(x => x.Name).ToList();
        }
    }
}
