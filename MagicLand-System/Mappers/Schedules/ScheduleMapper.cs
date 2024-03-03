using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
using MagicLand_System.Mappers.Attendances;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Schedules.ForLecturer;

namespace MagicLand_System.Mappers.Schedules
{
    public class ScheduleMapper : Profile
    {
        public ScheduleMapper()
        {
            //CreateMap<Class, ScheduleWithAttendanceResponse>()
            //    .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => SlotCustomMapper.fromSlotToSlotResponse(src.Slot!)))
            //    .ForMember(dest => dest.Room, opt => opt.MapFrom(src => RoomCustomMapper.fromRoomToRoomResponse(src.Room!)))
            //    .ForMember(dest => dest.DayOfWeeks, opt => opt.MapFrom(src => DateTimeHelper.GetDatesFromDateFilter(src.DayOfWeek)[0].ToString()))
            //    .ForMember(dest => dest.AttendanceInformation, opt => opt.MapFrom(src => src.Attendances));

            CreateMap<Schedule, ScheduleWithAttendanceResponse>()
               .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => SlotCustomMapper.fromSlotToSlotResponse(src.Slot!)))
               .ForMember(dest => dest.Room, opt => opt.MapFrom(src => RoomCustomMapper.fromRoomToRoomResponse(src.Room!)))
               .ForMember(dest => dest.DayOfWeeks, opt => opt.MapFrom(src => DateTimeHelper.GetDatesFromDateFilter(src.DayOfWeek)[0].ToString()))
               .ForMember(dest => dest.AttendanceInformation, opt => opt.MapFrom(src => src.Attendances));

            CreateMap<Schedule, ScheduleResponse>()
                  .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => SlotCustomMapper.fromSlotToSlotResponse(src.Slot!)))
                  .ForMember(dest => dest.Room, opt => opt.MapFrom(src => RoomCustomMapper.fromRoomToRoomResponse(src.Room!)))
                  .ForMember(dest => dest.DayOfWeeks, opt => opt.MapFrom(src => DateTimeHelper.GetDatesFromDateFilter(src.DayOfWeek)[0].ToString()));

        }
    }
}
