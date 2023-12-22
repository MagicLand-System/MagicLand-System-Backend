using MagicLand_System.Domain.Models;

namespace MagicLand_System.Services.Interfaces
{
    public interface ISlotService
    {
        Task<List<Slot>> GetSlots();
    }
}
