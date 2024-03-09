using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.Mappers.Custom
{
    public class SessionCustomMapper
    {

        public static List<SessionSyllabusResponse> fromSessionAndScheduleToSessionSyllabusResponse(List<Session> sessions, List<Schedule> schedules)
        {
            if (sessions == null || schedules == null)
            {
                return default!;
            }

            var responses = new List<SessionSyllabusResponse>();
            foreach (var session in sessions)
            {
                var schedule = schedules[session.NoSession - 1];
                responses.Add(new SessionSyllabusResponse
                {
                    OrderSession = session.NoSession,
                    Date = schedule.Date.ToString(),
                    DateOfWeek = DateTimeHelper.GetDatesFromDateFilter(schedule.DayOfWeek)[0].ToString(),
                    StartTime = TimeOnly.Parse(schedule.Slot!.StartTime),
                    EndTime = TimeOnly.Parse(schedule.Slot!.EndTime),
                    Contents = fromSessionDescriptionsToSessionContentResponse(session.SessionDescriptions!),
                });
            }

            return responses;
        }
        //public static SyllabusInforResponse fromTopicsToSyllabusInforResponse(ICollection<Topic>? topics)
        //{
        //    if (topics == null)
        //    {
        //        return default!;
        //    }

        //    var response = new SyllabusInforResponse
        //    {
        //        Sessions = fromTopicsToSessionResponses(topics),
        //    };

        //    return response;
        //}

        public static List<SessionSyllabusResponse> fromSessionsToSessionForSyllabusResponses(ICollection<Session> sessions)
        {
            if (sessions == null)
            {
                return default!;
            }

            var responses = new List<SessionSyllabusResponse>();
            foreach (var ses in sessions)
            {
                responses.Add(new SessionSyllabusResponse
                {
                    OrderSession = ses.NoSession,
                    Date = "Cần Truy Suất Qua Lớp",
                    DateOfWeek = "Cần Truy Suất Qua Lớp",
                    Contents = fromSessionDescriptionsToSessionContentResponse(ses.SessionDescriptions!),
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
