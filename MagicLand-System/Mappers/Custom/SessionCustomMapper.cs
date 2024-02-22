using MagicLand_System.Domain.Models;
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
                Sessions = fromTopicsToSyllabusSessionResponse(topics),
            };

            return response;
        }


        public static List<SyllabusSessionResponse> fromTopicsToSyllabusSessionResponse(ICollection<Topic> topics)
        {
            if (topics == null)
            {
                return default!;
            }

            var responses = new List<SyllabusSessionResponse>();

            foreach (var topic in topics)
            {
                responses.Add(new SyllabusSessionResponse
                {
                    TopicName = topic.Name!,
                    Sessions = fromSessionsToListSessionResponse(topic.Sessions),

                });
            }


            return responses;
        }


        public static List<SessionResponse> fromSessionsToListSessionResponse(ICollection<Session> sessions)
        {
            if (sessions == null)
            {
                return default!;
            }
            var responses = new List<SessionResponse>();

            foreach (var session in sessions)
            {
                responses.Add(new SessionResponse
                {
                    Order = session.NoSession,
                    Contents = fromSessionDescriptionsToSessionContentResponse(session.SessionDescriptions),
                });
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
                    Details = desc.Detail,

                });
            }

            return responses;
        }

        public static SessionResponse fromSessionToSessionResponse(Session session)
        {
            if (session == null)
            {
                return default!;
            }
            var response = new SessionResponse
            {
                Order = session.NoSession,
                Contents = fromSessionDescriptionsToSessionContentResponse(session.SessionDescriptions),
            };

            return response;
        }


    }
}
