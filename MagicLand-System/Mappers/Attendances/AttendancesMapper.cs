using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Attendances;
using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.Mappers.Attendances
{
    public class AttendancesMapper : Profile
    {
        public AttendancesMapper()
        {
            CreateMap<Attendance, AttendanceResponse>()
               .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
               .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student!.FullName))
               .ForMember(dest => dest.Day, opt => opt.MapFrom(src => src.Schedule!.Date))
               .ForMember(dest => dest.IsPresent, opt => opt.MapFrom(src => src.IsPresent));
        }
    }
}
