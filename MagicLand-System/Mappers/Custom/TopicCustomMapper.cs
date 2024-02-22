using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.Mappers.Custom
{
    public class TopicCustomMapper
    {
        public static TopicResponse fromTopicToTopicResponse(Topic topic, Session? session)
        {
            if (topic == null)
            {
                return new TopicResponse();
            }

            //TopicResponse response = new TopicResponse
            //{
            //    TopicName = topic!.Name ??= "Undefined",
            //    OrderNumber = topic.OrderNumber,
            //    Sessions = session != null && session != default
            //    ? SessionCustomMapper.fromSessionToSessionResponse(session)
            //    : new SessionResponse(),
            //};

            //return response;
            return null;
        }

        public static TopicWithSingleSession fromTopicToTopicWithSingleSessionResponse(Topic topic, Session? session)
        {
            if (topic == null)
            {
                return new TopicWithSingleSession();
            }

            var response = new TopicWithSingleSession
            {
                TopicName = topic!.Name ??= "Undefined",
                OrderNumber = topic.OrderNumber,
                Session = session != null && session != default
                ? SessionCustomMapper.fromSessionToSessionResponse(session)
                : default,
            };

            return response;
        }
    }
}
