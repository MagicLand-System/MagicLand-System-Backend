using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Custom
{
    public class QuestionPackageCustomMapper
    {
        public static List<QuestionPackageResponse> fromTopicsToQuestionPackageResponse(ICollection<Topic> topics)
        {
            if (topics == null)
            {
                return default!;
            }

            var responses = new List<QuestionPackageResponse>();
            var sessions = topics.SelectMany(tp => tp.Sessions!).ToList();

            foreach (var ses in sessions)
            {
                if(ses.QuestionPackage == null)
                {
                    continue;
                }

                responses.Add(new QuestionPackageResponse
                {
                    Title = ses.QuestionPackage!.Title,
                    Type = ses.QuestionPackage!.Type,
                    NoOfSession = ses.NoSession,
                });
            }

            return responses;
        }
    }
}

