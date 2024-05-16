using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain.Models.TempEntity.Quiz;
using MagicLand_System.Repository.Interfaces;

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

                    var newDeleteNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Xóa Lúc " + currentTime,
                    };

                    var tempQuiz = await _unitOfWork.GetRepository<TempQuiz>().GetListAsync(orderBy: x => x.OrderBy(x => x.CreatedTime));

                    foreach (var quiz in tempQuiz)
                    {
                        int time = currentTime.Day - quiz.CreatedTime.Day;

                        if (time >= 1)
                        {
                            _unitOfWork.GetRepository<TempQuiz>().DeleteAsync(quiz);
                        }
                    }

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
