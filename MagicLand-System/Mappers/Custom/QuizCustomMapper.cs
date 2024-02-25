using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Quizes;
using MagicLand_System.PayLoad.Response.Quizzes;

namespace MagicLand_System.Mappers.Custom
{
    public class QuizCustomMapper
    {

        public static QuizResponse fromSyllabusItemsToQuizResponse(int noSession, QuestionPackage questionPackage, ExamSyllabus examSyllabus)
        {
            if (questionPackage == null || examSyllabus == null)
            {
                return new QuizResponse { Date = "Cần Truy Suất Qua Lớp" };
            }

            var response = new QuizResponse
            {
                QuizCategory = examSyllabus.Category,
                QuizType = questionPackage.Type,
                QuizName = questionPackage.Title,
                Weight = examSyllabus.Weight,
                CompleteionCriteria = examSyllabus.CompleteionCriteria,
                TotalMark = questionPackage.Questions!.SelectMany(quest => quest.MutipleChoiceAnswers!.Select(mutiple => mutiple.Score).ToList()).Sum(),
                TotalQuestion = questionPackage.Questions!.Count(),
                Duration = examSyllabus.Duration,
                Attempt = 1,
                NoSession = noSession,
                ExamId = examSyllabus.Id,
                Questions = QuestionCustomMapper.fromQuestionPackageToQuestionResponse(questionPackage),
            };

            return response;
        }
        public static QuizMultipleChoiceResponse fromSyllabusItemsToQuizMutipleChoiceResponse(int noSession, QuestionPackage questionPackage, ExamSyllabus examSyllabus)
        {
            if (questionPackage == null || examSyllabus == null)
            {
                return new QuizMultipleChoiceResponse { Date = "Cần Truy Suất Qua Lớp" };
            }

            var response = new QuizMultipleChoiceResponse
            {
                QuizCategory = examSyllabus.Category,
                QuizType = questionPackage.Type,
                QuizName = questionPackage.Title,
                Weight = examSyllabus.Weight,
                CompleteionCriteria = examSyllabus.CompleteionCriteria,
                TotalMark = questionPackage.Questions!.SelectMany(quest => quest.MutipleChoiceAnswers!.Select(mutiple => mutiple.Score).ToList()).Sum(),
                TotalQuestion = questionPackage.Questions!.Count(),
                Duration = examSyllabus.Duration,
                Attempt = 1,
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

            var response = new QuizFlashCardResponse
            {
                QuizCategory = examSyllabus.Category,
                QuizType = questionPackage.Type,
                QuizName = questionPackage.Title,
                Weight = examSyllabus.Weight,
                CompleteionCriteria = examSyllabus.CompleteionCriteria,
                TotalMark = questionPackage.Questions!.SelectMany(quest => quest.FlashCards!.Select(fc => fc.Score)).ToList().Sum(),
                TotalQuestion = questionPackage.Questions!.Count(),
                Duration = examSyllabus.Duration,
                Attempt = 1,
                NoSession = noSession,
                ExamId = examSyllabus.Id,
                QuestionFlasCards = QuestionCustomMapper.fromQuestionPackageToQuestionFlashCardResponse(questionPackage),
            };

            return response;
        }
    }
}
