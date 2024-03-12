using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.Mappers.Custom
{
    public class TopicCustomMapper
    {
        public static List<TopicResponse> fromTopicsToTopicResponses(ICollection<Topic> topics)
        {
            if (topics == null)
            {
                return new List<TopicResponse>();
            }

            var responses = new List<TopicResponse>();

            foreach (var topic in topics)
            {
                responses.Add(new TopicResponse
                {
                    TopicName = topic.Name,
                    OrderTopic = topic.OrderNumber,
                    Sessions = SessionCustomMapper.fromSessionsToSessionForSyllabusResponses(topic.Sessions!),
                });
            }

            return responses;
        }

       

        public static List<TopicResponse> fromTopicsAndScheduleToTopicResponses(ICollection<Topic> topics, ICollection<Schedule> schedules)
        {
            if (topics == null)
            {
                return new List<TopicResponse>();
            }

            var responses = new List<TopicResponse>();

            foreach (var topic in topics)
            {
                responses.Add(new TopicResponse
                {
                    TopicName = topic.Name,
                    OrderTopic = topic.OrderNumber,
                    Sessions = SessionCustomMapper.fromSessionAndScheduleToSessionSyllabusResponse(topic.Sessions!.ToList(), schedules.ToList()),
                });
            }

            return responses;
        }
    }
}
