using AutoMapper;
using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.Mappers.Topics
{
    public class TopicMapper : Profile
    {

        public TopicMapper()
        {
            CreateMap<MagicLand_System.Domain.Models.Topic, TopicResponse>()
                .ForMember(dest => dest.Session, opt => opt.MapFrom(src => src.Sessions));

        }
    }
}
