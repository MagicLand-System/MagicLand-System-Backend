using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain.Models.TempEntity.Quiz;
using MagicLand_System.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Background.BackgroundServiceImplements
{
    public class TempEntityBackgroundService : ITempEntityBackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TempEntityBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<string> DeleteTempEntityByCondition()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentTime = BackgoundTime.GetTime();
                    var deleteMCAnswers = new List<MultipleChoiceAnswer>();
                    var newDeleteNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Xóa Lúc " + currentTime,
                    };

                    var tempExams = await _unitOfWork.GetRepository<ExamResult>().GetListAsync(predicate: x => x.IsGraded == false, include: x => x.Include(x => x.ExamQuestions));

                    //foreach (var exam in tempExams)
                    //{
                    //    var examQuestionIds = exam.ExamQuestions.Select(x => x.Id).ToList();

                    //    var multipleChoiceAnswers = await _unitOfWork.GetRepository<MultipleChoiceAnswer>().GetListAsync(predicate: x => examQuestionIds.Contains(x.ExamQuestionId));
                    //    if (multipleChoiceAnswers.Any())
                    //    {
                    //        deleteMCAnswers.AddRange(multipleChoiceAnswers);
                    //    }
                    //}
                    if (tempExams.Any())
                    {
                        _unitOfWork.GetRepository<ExamResult>().DeleteRangeAsync(tempExams);
                    }

                    //_unitOfWork.GetRepository<MultipleChoiceAnswer>().DeleteRangeAsync(deleteMCAnswers);
                    await _unitOfWork.GetRepository<Notification>().InsertAsync(newDeleteNotification);
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Delete Temp Entity Got An Error: [{ex.Message}]";
            }
            return "Delete Temp Entity Success";
        }


    }
}
