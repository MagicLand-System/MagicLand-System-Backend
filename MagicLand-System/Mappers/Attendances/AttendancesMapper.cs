using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.Mappers.Schedules;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Schedules;
using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.Mappers.Attendances
{
    public class AttendancesMapper : Profile
    {
        public AttendancesMapper()
        {

            CreateMap<Class, AttendanceWithClassResponse>()
              .ForMember(dest => dest.ClassId, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.ClassCode))
              .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.Lecture, opt => opt.MapFrom(src => UserCustomMapper.fromUserToUserResponse(src.Lecture!)))
              .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules.Select(sc => 
              ScheduleCustomMapper.fromClassScheduleToScheduleWithAttendanceResponse(sc)).ToList()));


            CreateMap<Attendance, AttendanceResponse>()
               .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
               .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student!.FullName))
               .ForMember(dest => dest.IsPresent, opt => opt.MapFrom(src => src.IsPresent));
        }
    }
}
