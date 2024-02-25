using Azure;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Quizzes.Answers;
using MagicLand_System.PayLoad.Response.Quizzes.Questions;
using MagicLand_System.PayLoad.Response.Syllabuses;

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
                responses.Add(new QuestionMutipleChoiceResponse
                {
                    QuestionId = question.Id,
                    QuestionDescription = question.Description,
                    QuestionImage = question.Img,
                    Answers = fromMutipleChoiceAnswerToMutipleChoiceAnswerResponse(question.MutipleChoiceAnswers!),
                });
            }

            return responses;
        }

        public static List<MutilpleChoiceAnswerResponse> fromMutipleChoiceAnswerToMutipleChoiceAnswerResponse(List<MutipleChoiceAnswer> answers)
        {
            if (answers == null)
            {
                return default!;
            }

            var responses = new List<MutilpleChoiceAnswerResponse>();

            foreach (var answer in answers)
            {
                responses.Add(new MutilpleChoiceAnswerResponse
                {
                    AnswerId = answer.Id,
                    AnswerDescription = answer.Description,
                    AnswerImage = answer.Img,
                    Score = answer.Score,
                });

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
                var questionFlashCard = new QuestionFlashCardResponse
                {
                    QuestionId = question.Id,
                    QuestionDescription = question.Description,
                    QuestionImage = question.Img,
                    FlashCars = fromFlashCardToFlashCardAnswerResponse(question.FlashCards!),
                };

                responses.Add(questionFlashCard);
            }

            return responses;
        }

        public static List<FlashCardAnswerResponse> fromFlashCardToFlashCardAnswerResponse(List<FlashCard> flashCards)
        {
            if (flashCards == null)
            {
                return default!;
            }

            var responses = new List<FlashCardAnswerResponse>();

            foreach (var flashCard in flashCards)
            {
                var response = new FlashCardAnswerResponse();
                foreach (var sideFlashCard in flashCard.SideFlashCards!)
                {

                    if (sideFlashCard.Side == "Left")
                    {
                        response.FirstCardId = sideFlashCard.Id;
                        response.FirstCardInfor = !string.IsNullOrEmpty(sideFlashCard.Image) ? sideFlashCard.Image : sideFlashCard.Description;
                        continue;
                    }
                    if (sideFlashCard.Side == "Right")
                    {
                        response.SecondCardId = sideFlashCard.Id;
                        response.SecondCardInfor = !string.IsNullOrEmpty(sideFlashCard.Image) ? sideFlashCard.Image : sideFlashCard.Description;
                    }
                }

                response.Score = flashCard.Score;
                responses.Add(response);
            }

            return responses;
        }

        public static List<QuestionResponse> fromQuestionPackageToQuestionResponse(QuestionPackage package)
        {
            if (package == null)
            {
                return default!;
            }

            var responses = new List<QuestionResponse>();

            foreach (var question in package.Questions!)
            {
                var response = new QuestionResponse
                {
                    QuestionId = question.Id,
                    QuestionDescription = question.Description,
                    QuestionImage = question.Img,
                    AnswersMutipleChoicesInfor = question.MutipleChoiceAnswers!.Any() ? fromMutipleChoiceAnswerToMutipleChoiceAnswerResponse(question.MutipleChoiceAnswers!) : default,
                    AnwserFlashCarsInfor = question.FlashCards!.Any() ? fromFlashCardToFlashCardAnswerResponse(question.FlashCards!) : default,
                };

                responses.Add(response);
            }

            return responses;
        }

    }
}

