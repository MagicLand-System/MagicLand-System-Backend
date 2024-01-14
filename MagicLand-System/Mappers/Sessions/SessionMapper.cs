using AutoMapper;
using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.Mappers.Sessions
{
    public class SessionMapper : Profile
    {
        public SessionMapper()
        {
            CreateMap<MagicLand_System.Domain.Models.Session, SessionResponse>();
        }
    }
}
