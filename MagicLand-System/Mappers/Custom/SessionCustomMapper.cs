using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Sessions;

namespace MagicLand_System.Mappers.Custom
{
    public class SessionCustomMapper
    {
        public static SessionResponse fromSessionToSessionResponse(Session session)
        {
            if (session == null)
            {
                return new SessionResponse();
            }

            SessionResponse response = new SessionResponse
            {
                Content = session.Content ??= string.Empty,
                Description = session.Description ??= string.Empty
            };

            return response;
        }
    }
}
