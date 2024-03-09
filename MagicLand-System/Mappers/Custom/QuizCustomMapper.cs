using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;

namespace MagicLand_System.Mappers.Custom
{
    public class QuizCustomMapper
    {

        public static ExamWithQuizResponse fromSyllabusItemsToQuizWithQuestionResponse(int noSession, QuestionPackage questionPackage, ExamSyllabus? examSyllabus)
        {
            if (questionPackage == null)
            {
                return new ExamWithQuizResponse { Date = "Cần Truy Suất Qua Lớp" };
            }

            int part = 1;
            if (questionPackage.Type == "flashcard")
            {
                part = 2;
            }

            var response = new ExamWithQuizResponse
            {
                ExamPart = part,
                QuizCategory = examSyllabus == null ? "Review" : examSyllabus.Category,
                QuizType = questionPackage.Type,
                QuizName = questionPackage.Title,
                Weight = examSyllabus == null ? 0 : examSyllabus.Weight ,
                CompleteionCriteria = examSyllabus == null ? 0 : examSyllabus.CompleteionCriteria,
                TotalScore = questionPackage.Questions!.SelectMany(quest => quest.MutipleChoiceAnswers!.Select(mutiple => mutiple.Score).ToList()).Sum(),
                TotalQuestion = questionPackage.Questions!.Count(),
                //Duration = questionPackage.Duration,
                //DeadLine = questionPackage.DeadlineTime,
                //Attempts = questionPackage.AttemptsAllowed,
                NoSession = noSession,
                ExamId = questionPackage.Id,
                Quizzes = QuestionCustomMapper.fromQuestionPackageToQuizResponse(questionPackage),

            };

            return response;
        }

        public static ExamResponse fromSyllabusItemsToExamResponse(int noSession, QuestionPackage questionPackage, ExamSyllabus examSyllabus)
        {
            if (questionPackage == null || examSyllabus == null)
            {
                return new ExamWithQuizResponse { Date = "Cần Truy Suất Qua Lớp" };
            }


            int part = 1;
            if (questionPackage.Type == "flashcard")
            {
                part = 2;
            }

            var quizzes = QuestionCustomMapper.fromQuestionPackageToQuizResponseInLimitScore(questionPackage);

            return new ExamResponse
            {
                ExamPart = part,
                ExamName = "Bài Kiểm Tra Số " + questionPackage.OrderPackage,
                QuizCategory = examSyllabus.Category,
                QuizType = questionPackage.Type,
                QuizName = questionPackage.Title,
                Weight = examSyllabus.Weight,
                CompleteionCriteria = examSyllabus.CompleteionCriteria,
                TotalScore = (double)questionPackage.Score!,
                TotalQuestion = quizzes.Count(),
                //Duration = questionPackage.Duration,
                //DeadLine = questionPackage.DeadlineTime,
                //Attempts = questionPackage.AttemptsAllowed,
                NoSession = noSession,
                ExamId = examSyllabus.Id,
            };
        }

        public static QuizMultipleChoiceResponse fromSyllabusItemsToQuizMutipleChoiceResponse(int noSession, QuestionPackage questionPackage, ExamSyllabus examSyllabus)
        {
            if (questionPackage == null || examSyllabus == null)
            {
                return new QuizMultipleChoiceResponse { Date = "Cần Truy Suất Qua Lớp" };
            }


            int part = 1;
            if (questionPackage.Type == "flashcard")
            {
                part = 2;
            }

            var response = new QuizMultipleChoiceResponse
            {
                ExamPart = part,
                QuizCategory = examSyllabus.Category,
                QuizType = questionPackage.Type,
                QuizName = questionPackage.Title,
                Weight = examSyllabus.Weight,
                CompleteionCriteria = examSyllabus.CompleteionCriteria,
                TotalScore = questionPackage.Questions!.SelectMany(quest => quest.MutipleChoiceAnswers!.Select(mutiple => mutiple.Score).ToList()).Sum(),
                TotalQuestion = questionPackage.Questions!.Count(),
                //Attempts = 1,
                NoSession = noSession,
                ExamId = examSyllabus.Id,
                QuestionMultipleChoices = QuestionCustomMapper.fromQuestionPackageToQuestionMultipleChoicesResponse(questionPackage),
            };

            return response;
        }
        public static QuizFlashCardResponse fromSyllabusItemsToQuizFlashCardResponse(int noSession, QuestionPackage questionPackage, ExamSyllabus examSyllabus)
        {
            if (questionPackage == null || examSyllabus == null)
            {
                return new QuizFlashCardResponse { Date = "Cần Truy Suất Qua Lớp", };
            }


            int part = 1;
            if (questionPackage.Type == "flashcard")
            {
                part = 2;
            }

            var response = new QuizFlashCardResponse
            {
                ExamPart = part,
                QuizCategory = examSyllabus.Category,
                QuizType = questionPackage.Type,
                QuizName = questionPackage.Title,
                Weight = examSyllabus.Weight,
                CompleteionCriteria = examSyllabus.CompleteionCriteria,
                TotalScore = questionPackage.Questions!.SelectMany(quest => quest.FlashCards!.Select(fc => fc.Score)).ToList().Sum(),
                TotalQuestion = questionPackage.Questions!.Count(),
                //Attempts = 1,
                NoSession = noSession,
                ExamId = examSyllabus.Id,
                QuestionFlasCards = QuestionCustomMapper.fromQuestionPackageToQuestionFlashCardResponse(questionPackage),
            };

            return response;
        }
    }
}
