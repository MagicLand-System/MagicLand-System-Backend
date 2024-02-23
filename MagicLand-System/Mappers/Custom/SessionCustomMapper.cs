using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.Mappers.Custom
{
    public class SessionCustomMapper
    {
        public static SyllabusInforResponse fromTopicsToSyllabusInforResponse(ICollection<Topic>? topics)
        {
            if (topics == null)
            {
                return default!;
            }

            var response = new SyllabusInforResponse
            {
                Sessions = fromTopicsToSessionResponses(topics),
            };

            return response;
        }


        public static List<SessionResponse> fromTopicsToSessionResponses(ICollection<Topic> topics)
        {
            if (topics == null)
            {
                return default!;
            }

            var responses = new List<SessionResponse>();

            foreach (var topic in topics)
            {
                foreach(var session in topic.Sessions!)
                {
                    responses.Add(new SessionResponse
                    {
                        OrderTopic = topic.OrderNumber,
                        OrderSession = session.NoSession,
                        TopicName = topic.Name!,
                        Contents = fromSessionDescriptionsToSessionContentResponse(session.SessionDescriptions!),
                    });
                }        
            }
            return responses;
        }


        public static List<SessionContentReponse> fromSessionDescriptionsToSessionContentResponse(ICollection<SessionDescription> descriptions)
        {
            if (descriptions == null)
            {
                return default!;
            }

            var responses = new List<SessionContentReponse>();

            foreach (var desc in descriptions)
            {
                responses.Add(new SessionContentReponse
                {
                    Content = desc.Content,
                    Details = StringHelper.FromStringToList(desc.Detail!),
                });
            }

            return responses;
        }

        public static SessionResponse fromSessionToSessionResponse(Session session, Topic topic)
        {
            if (session == null)
            {
                return new SessionResponse();
            }

            var response = new SessionResponse
            {
                OrderTopic = topic.OrderNumber,
                OrderSession = session.NoSession,
                TopicName = topic.Name,
                Contents = fromSessionDescriptionsToSessionContentResponse(session.SessionDescriptions!),
            };

            return response;
        }


    }
}
