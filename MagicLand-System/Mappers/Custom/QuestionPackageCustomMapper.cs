using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Syllabuses;

namespace MagicLand_System.Mappers.Custom
{
    public class QuestionPackageCustomMapper
    {
        public static QuestionPackageResponse fromTopicsToQuestionPackageResponse(ICollection<Topic> topics)
        {
            if (topics == null)
            {
                return default!;
            }

            var questions = new List<QuestionPackageInforResponse>();
            var sessions = topics.SelectMany(tp => tp.Sessions).ToList();

            if(sessions.Any(ses => ses.QuestionPackage == null))
            {
                return default!;
            }

            foreach(var session in sessions)
            {

                questions.Add(new QuestionPackageInforResponse
                {
                    Title = session.QuestionPackage!.Title,
                    Type = session.QuestionPackage!.Type,
                    NoOfSession = session.NoSession,
                });
            }      

            return new QuestionPackageResponse { QuestionInfor = questions};
        }
    }
}
