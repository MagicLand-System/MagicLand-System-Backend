using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Mappers.CustomMapper;
using MagicLand_System.PayLoad.Response;

namespace MagicLand_System.Mappers
{
    public class ObjectMapper : Profile
    {
        public ObjectMapper()
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
                .ForMember(dest => dest.NumberStudentRegistered, opt => opt.MapFrom(src => src.ClasssTransactions.Count()))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address ?? new Address()))
                .ForMember(dest => dest.Lecture, opt => opt.MapFrom(src => CustomMapper.CustomMapper.fromUserToUserResponse(src.User)));

            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address ?? new Address()));

            CreateMap<Address, AddressResponse>();
        }
    }
}
