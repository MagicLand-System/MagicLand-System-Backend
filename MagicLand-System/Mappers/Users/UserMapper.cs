using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response;

namespace MagicLand_System.Mappers.Users
{
    public class UserMapper : Profile
    {
        public UserMapper() {
            CreateMap<User, LoginResponse>()
                .ForMember(des => des.AccessToken, src => src.Ignore())
                .ForMember(des => des.Role, src => src.MapFrom(src => src.Role.Name));       
        }
    }
}
