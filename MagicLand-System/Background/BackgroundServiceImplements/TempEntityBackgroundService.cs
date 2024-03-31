using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Domain.Models.TempEntity.Quiz;
using MagicLand_System.Domain.Models.TempEntity.Class;

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
                    var currentDate = DateTime.Now;

                    var tempQuiz = await _unitOfWork.GetRepository<TempQuiz>().GetListAsync(orderBy: x => x.OrderBy(x => x.CreatedTime));

                    foreach (var quiz in tempQuiz)
                    {
                        int time = currentDate.Day - quiz.CreatedTime.Day;

                        if (time >= 1)
                        {
                            _unitOfWork.GetRepository<TempQuiz>().DeleteAsync(quiz);
                        }
                    }
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
