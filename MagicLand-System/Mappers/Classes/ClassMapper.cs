using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.PayLoad.Request.Student;
using MagicLand_System.PayLoad.Response;
using MagicLand_System.PayLoad.Response.Address;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Session;

namespace MagicLand_System.Mappers.Classes
{
    public class ClassMapper : Profile
    {
        public ClassMapper()
        {
            CreateMap<User, LoginResponse>()
               .ForMember(des => des.AccessToken, src => src.Ignore())
               .ForMember(des => des.Role, src => src.MapFrom(src => src.Role.Name));


            CreateMap<Class, ClassResponse>()
                .ForMember(dest => dest.LimitNumberStudent, opt => opt.MapFrom(src => src.LimitNumberStudent))
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method.ToString().Equals(ClassEnum.ONLINE.ToString())
                ? ClassEnum.ONLINE.ToString()
                : ClassEnum.OFFLINE.ToString()))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => (decimal)src.Price))
                //.ForMember(dest => dest.NumberStudentRegistered, opt => opt.MapFrom(src => src.ClasssTransactions.Count()))
                //.ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address ?? new Address()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
                //.ForMember(dest => dest.Lecture, opt => opt.MapFrom(src => CustomMapper.CustomMapper.fromUserToUserResponse(src.User)))
                //.ForMember(dest => dest.Sessions, opt => opt.MapFrom(src => src.Sessions
                //.Select(s => CustomMapper.CustomMapper.fromSessionToSessionResponse(s)) ?? new List<SessionResponse>()));

            CreateMap<Address, AddressResponse>();
        }
    }
}
