using Azure;
using MagicLand_System.Domain.Models;
using MagicLand_System.Helpers;
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

        public static List<QuestionMCResponse> fromQuestionPackageToQuestionMultipleChoicesResponse(QuestionPackage package)
        {
            if (package == null)
            {
                return default!;
            }

            var responses = new List<QuestionMCResponse>();

            foreach (var question in package.Questions!)
            {
                responses.Add(new QuestionMCResponse
                {
                    QuestionId = question.Id,
                    QuestionDescription = question.Description,
                    QuestionImage = question.Img,
                    Answers = fromMutipleChoiceAnswerToMutipleChoiceAnswerResponse(question.MutipleChoices!),
                });
            }

            return responses;
        }

        public static List<MCAnswerResponse> fromMutipleChoiceAnswerToMutipleChoiceAnswerResponse(List<MultipleChoice> answers)
        {
            if (answers == null)
            {
                return default!;
            }

            var responses = new List<MCAnswerResponse>();

            foreach (var answer in answers)
            {
                responses.Add(new MCAnswerResponse
                {
                    AnswerId = answer.Id,
                    AnswerDescription = answer.Description,
                    AnswerImage = answer.Img,
                    Score = answer.Score,
                });

            }
            return responses;
        }

        public static List<QuestionFCResponse> fromQuestionPackageToQuestionFlashCardResponse(QuestionPackage package)
        {
            if (package == null)
            {
                return default!;
            }

            var responses = new List<QuestionFCResponse>();

            //foreach (var question in package.Questions!)
            //{
            //    var questionFlashCard = new QuestionFlashCardResponse
            //    {
            //        QuestionId = question.Id,
            //        QuestionDescription = question.Description,
            //        QuestionImage = question.Img,
            //        FlashCars = fromFlashCardToFlashCardAnswerResponse(question.FlashCards!),
            //    };

            //    responses.Add(questionFlashCard);
            //}

            return responses;
        }

        public static List<FCAnswerResponse> fromFlashCardToFlashCardAnswerResponse(List<FlashCard> flashCards)
        {
            if (flashCards == null)
            {
                return default!;
            }

            var responses = new List<FCAnswerResponse>();

            for (int i = 0; i < flashCards.Count(); i++)
            {
                foreach (var sideFlashCard in flashCards[i].SideFlashCards!)
                {
                    var response = new FCAnswerResponse
                    {
                        CardId = sideFlashCard.Id,
                        CardDescription = !string.IsNullOrEmpty(sideFlashCard.Description) ? sideFlashCard.Description : "",
                        CardImage = !string.IsNullOrEmpty(sideFlashCard.Image) ? sideFlashCard.Image : "",
                        Score = flashCards[i].Score / 2,
                        NumberCoupleIdentify = i,
                    };
                    responses.Add(response);
                }
            }

            return responses;
        }

        public static List<QuizResponse> fromQuestionPackageToQuizResponse(QuestionPackage package)
        {
            if (package == null)
            {
                return default!;
            }

            var responses = new List<QuizResponse>();

            foreach (var question in package.Questions!)
            {
                var response = new QuizResponse
                {
                    QuestionId = question.Id,
                    QuestionDescription = question.Description,
                    QuestionImage = question.Img,
                    AnswersMutipleChoicesInfor = question.MutipleChoices!.Any() ? fromMutipleChoiceAnswerToMutipleChoiceAnswerResponse(question.MutipleChoices!) : default,
                    AnwserFlashCarsInfor = question.FlashCards!.Any() ? fromFlashCardToFlashCardAnswerResponse(question.FlashCards!) : default,
                };

                responses.Add(response);
            }

            return responses;
        }

        public static List<QuizResponse>? fromQuestionPackageToQuizResponseInLimitScore(QuestionPackage package)
        {
            if (package == null || package.Score == 0)
            {
                return null;
            }

            Random random = new Random();
            var questions = package.Questions;
            double totalMark = 0.0;
            bool isBreak = false;
            var responses = new List<QuizResponse>();
            var usedIndices = new HashSet<int>();

            while (!isBreak)
            {
                int randomQuestionIndex;
                do
                {
                    randomQuestionIndex = random.Next(0, questions!.Count);
                } while (usedIndices.Contains(randomQuestionIndex));

                usedIndices.Add(randomQuestionIndex);

                var currentQuestion = questions![randomQuestionIndex];
                var answerMutipleChoicesInfor = new List<MCAnswerResponse>();
                var answerFlashCarsInfor = new List<FCAnswerResponse>();

                if (currentQuestion.MutipleChoices != null && currentQuestion.MutipleChoices.Any())
                {
                    answerMutipleChoicesInfor = fromMutipleChoiceAnswerToMutipleChoiceAnswerResponse(currentQuestion.MutipleChoices!);
                    totalMark += currentQuestion.MutipleChoices!.Select(mc => mc.Score).Sum();
                }
                if (currentQuestion.FlashCards != null && currentQuestion.FlashCards.Any())
                {
                    int coupleFlashCardLeft = (int)(package.Score - totalMark)!;
                    var listFlashCard = new List<FlashCard>();
                    var usedCouple = new HashSet<int>();

                    for (int i = 0; i < coupleFlashCardLeft && i < currentQuestion.FlashCards.Count(); i++)
                    {
                        int randomCouple;
                        do
                        {
                            randomCouple = random.Next(0, currentQuestion.FlashCards.Count());
                        } while (usedCouple.Contains(randomCouple));

                        listFlashCard.Add(currentQuestion.FlashCards[randomCouple]);
                    }
                    answerFlashCarsInfor = fromFlashCardToFlashCardAnswerResponse(listFlashCard);

                    totalMark += listFlashCard.Select(fc => fc.Score).Sum();
                }

                var response = new QuizResponse
                {
                    QuestionId = currentQuestion.Id,
                    QuestionDescription = currentQuestion.Description,
                    QuestionImage = currentQuestion.Img,
                    AnswersMutipleChoicesInfor = answerMutipleChoicesInfor,
                    AnwserFlashCarsInfor = answerFlashCarsInfor,
                };

                if (totalMark == package.Score)
                {
                    isBreak = true;
                }

                responses.Add(response);
            }

            return responses;
        }


    }
}

