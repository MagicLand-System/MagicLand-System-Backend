using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Quizzes;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Utils;

namespace MagicLand_System.Mappers.Custom
{
    public class QuestionCustomMapper
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
                if (ses.QuestionPackage == null)
                {
                    continue;
                }

                responses.Add(new QuestionPackageResponse
                {
                    QuestionPackageId = ses.QuestionPackage.Id,
                    Title = ses.QuestionPackage!.Title,
                    Type = ses.QuestionPackage!.Type,
                    NoOfSession = ses.NoSession,
                });
            }

            return responses;
        }

        public static List<QuestionMutipleChoiceResponse> fromQuestionPackageToQuestionMultipleChoicesResponse(QuestionPackage package)
        {
            if (package == null)
            {
                return default!;
            }

            var responses = new List<QuestionMutipleChoiceResponse>();

            foreach (var question in package.Questions!)
            {
                foreach (var answer in question.MutipleChoiceAnswers!)
                {
                    responses.Add(new QuestionMutipleChoiceResponse
                    {
                        QuestionDescription = question.Description,
                        QuestionImage = question.Img,
                        AnswerImage = answer.Img,
                        Answer = answer.Description,
                        Score = answer.Score,
                    });
                }
            }

            return responses;
        }

        public static List<QuestionFlashCardResponse> fromQuestionPackageToQuestionFlashCardResponse(QuestionPackage package)
        {
            if (package == null)
            {
                return default!;
            }

            var responses = new List<QuestionFlashCardResponse>();

            foreach (var question in package.Questions!)
            {
                foreach (var flashCard in question.FlashCards!)
                {
                    var questionFlashCard = new QuestionFlashCardResponse
                    {
                        QuestionDescription = question.Description,
                        QuestionImage = question.Img,
                        Score = flashCard.Score,
                    };

                    foreach (var sideFlashCard in flashCard.SideFlashCards!)
                    {
                        if (sideFlashCard.Side == "Left")
                        {
                            questionFlashCard.CardQuestion = sideFlashCard.Image != null ? sideFlashCard.Image : sideFlashCard.Description;
                            continue;
                        }

                        if (sideFlashCard.Side == "Right")
                        {
                            questionFlashCard.CardAnswer = sideFlashCard.Image != null ? sideFlashCard.Image : sideFlashCard.Description;
                        }
                    }
                    responses.Add(questionFlashCard);
                }
            }

            return responses;
        }
    }
}

