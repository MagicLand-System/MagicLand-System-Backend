using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Slots;

namespace MagicLand_System.Mappers.Custom
{
    public class SlotCustomMapper
    {
        public static SlotResponse fromSlotToSlotResponse(Slot slot)
        {
            if (slot == null)
            {
                return new SlotResponse();
            }

            SlotResponse response = new SlotResponse
            {
                SlotId = slot.Id,
                StartTime = TimeOnly.Parse(slot.StartTime),
                EndTime = TimeOnly.Parse(slot.EndTime)
            };

            return response;
        }
    }
}
