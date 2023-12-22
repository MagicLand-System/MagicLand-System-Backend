using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;

namespace MagicLand_System.Services.Implements
{
    public class SlotService : BaseService<SlotService>, ISlotService
    {
        public SlotService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<SlotService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<List<Slot>> GetSlots()
        {
            var slots = (await _unitOfWork.GetRepository<Slot>().GetListAsync()).ToList();
            if(slots == null || slots.Count == 0)
            {
                return null;
            }
            return slots;
        }
    }
}
