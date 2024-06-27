using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Response.Slots;
using MagicLand_System.Utils;

namespace MagicLand_System.Mappers.Custom
{
    public class SlotCustomMapper
    {
        public static SlotReponseForLecture fromSlotToSlotForLecturerResponse(Slot slot)
        {
            if (slot == null)
            {
                return new SlotReponseForLecture();
            }

            SlotReponseForLecture response = new SlotReponseForLecture
            {
                SlotId = slot.Id,
                StartTime = TimeOnly.Parse(slot.StartTime),
                EndTime = TimeOnly.Parse(slot.EndTime)
            };

            return response;
        }
        public static SlotResponse fromSlotToSlotResponse(Slot slot)
        {
            if (slot == null)
            {
                return new SlotResponse();
            }

            SlotResponse response = new SlotResponse
            {
                SlotId = slot.Id,
                SlotOrder = StringHelper.GetSlotNumber(slot.StartTime),
                StartTime = TimeOnly.Parse(slot.StartTime),
                EndTime = TimeOnly.Parse(slot.EndTime)
            };

            return response;
        }

      
    }
}
