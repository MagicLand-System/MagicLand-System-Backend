using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Response.Sessions;
using MagicLand_System.PayLoad.Response.Syllabuses;
using Quartz;

namespace MagicLand_System.Mappers.Custom
{
    public class SessionCustomMapper
    {

        public static SyllabusInforWithDateResponse fromTopicsAndSchedulesToSyllabusInforWithDateResponse(ICollection<Topic>? topics, ICollection<Schedule> schedules)
        {
            if (topics == null || schedules == null)
            {
                return default!;
            }

            var response = new SyllabusInforWithDateResponse
            {
                Sessions = fromTopicsAndSchedulesToSessionWithDateResponses(topics, schedules.ToList()),
            };

            return response;
        }
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


        public static List<SessionWithDateResponse> fromTopicsAndSchedulesToSessionWithDateResponses(ICollection<Topic> topics, List<Schedule> schedules)
        {
            if (topics == null || schedules == null)
            {
                return default!;
            }

            var responses = new List<SessionWithDateResponse>();

            foreach (var topic in topics)
            {
                foreach (var session in topic.Sessions!)
                {
                    int index = session.NoSession - 1;
                    responses.Add(new SessionWithDateResponse
                    {
                        Date = schedules[index].Date,
                        DateOfWeek = DateTimeHelper.GetDatesFromDateFilter(schedules[index].DayOfWeek)[0].ToString(),
                        StartTime = TimeOnly.Parse(schedules[index].Slot!.StartTime),
                        EndTime = TimeOnly.Parse(schedules[index].Slot!.EndTime),
                        OrderTopic = topic.OrderNumber,
                        OrderSession = session.NoSession,
                        TopicName = topic.Name!,
                        Contents = fromSessionDescriptionsToSessionContentResponse(session.SessionDescriptions!),
                    }); ;
                }
            }
            return responses;
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
                foreach (var session in topic.Sessions!)
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
