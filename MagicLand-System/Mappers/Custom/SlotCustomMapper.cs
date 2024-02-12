using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Response.Slots;
using MagicLand_System.Utils;

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
                SlotOrder = GetSlotNumber(slot.StartTime),
                StartTime = TimeOnly.Parse(slot.StartTime),
                EndTime = TimeOnly.Parse(slot.EndTime)
            };

            return response;
        }

        private static string GetSlotNumber(string startTime)
        {
            if(startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot1).Trim())
            {
                return "Tiết 1";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot2).Trim())
            {
                return "Tiết 2";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot3).Trim())
            {
                return "Tiết 3";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot4).Trim())
            {
                return "Tiết 4";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot5).Trim())
            {
                return "Tiết 5";
            }
            if (startTime == EnumUtil.GetDescriptionFromEnum(SlotEnum.Slot6).Trim())
            {
                return "Tiết 6";
            }
            return "Undefined";
        }
    }
}
