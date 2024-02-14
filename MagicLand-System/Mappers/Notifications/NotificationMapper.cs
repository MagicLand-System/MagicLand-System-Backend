using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Response.Notifications;
using MagicLand_System.PayLoad.Response.Schedules;
using Newtonsoft.Json;

namespace MagicLand_System.Mappers.Notifications
{
    public class NotificationMapper : Profile
    {
        public NotificationMapper()
        {
            CreateMap<Notification, NotificationResponse>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
