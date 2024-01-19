using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Users;

namespace MagicLand_System.Mappers.Users
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<User, UserResponse>()
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
           .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
           .ForMember(dest => dest.AvatarImage, opt => opt.MapFrom(src => src.AvatarImage))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
           .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
           .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
           .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.City + " " + src.District + " " + src.Street));
        }

    }
}
