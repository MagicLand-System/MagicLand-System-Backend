using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Helpers;
using MagicLand_System.PayLoad.Response.Evaluates;
using MagicLand_System.PayLoad.Response.Syllabuses;
using MagicLand_System.Utils;

namespace MagicLand_System.Mappers.Custom
{
    public class EvaluateCustomMapper
    {
        public static EvaluateResponse fromEvaluateInforRelatedToEvaluateResponse(Schedule schedule, List<Evaluate> evaluates, int noSession)
        {
            if (schedule == null || evaluates == null)
            {
                return default!;
            }

            var response = new EvaluateResponse
            {
                Date = schedule.Date,
                NoSession = noSession,
                EvaludateInfors = fromEvaluateToEvaluateStudentResponse(evaluates),
            };

            return response;
        }

        public static List<EvaluateStudentResponse> fromEvaluateToEvaluateStudentResponse(List<Evaluate> evaluates)
        {
            if (evaluates == null)
            {
                return default!;
            }

            var response = new List<EvaluateStudentResponse>();
            foreach (var evaluate in evaluates)
            {
                response.Add(new EvaluateStudentResponse
                {
                    StudentId = evaluate.StudentId,
                    Level = evaluate.Status == EvaluateStatusEnum.NOTGOOD.ToString() 
                    ? 1 : evaluate.Status == EvaluateStatusEnum.NORMAL.ToString() 
                    ? 2 : evaluate.Status == EvaluateStatusEnum.GOOD.ToString() ? 3 : 0,
                    EvaluateDescription = evaluate.Status != null ? EnumUtil.CompareAndGetDescription<EvaluateStatusEnum>(evaluate.Status!) : "Chưa Đánh Giá",
                    Note = evaluate.Note,
                });
            }
            return response;
        }

    }
}
