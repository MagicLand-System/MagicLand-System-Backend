using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Background.BackgroundServiceImplements
{
    public class NotificationBackgroundService : INotificationBackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public NotificationBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

       public async Task<string> CreateNotificationInCondition()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentDate = DateTime.Now;

                    var classes = await _unitOfWork.GetRepository<Class>()
                      .GetListAsync(predicate: x => x.Status == ClassStatusEnum.CANCELED.ToString() || x.Status == ClassStatusEnum.PROGRESSING.ToString(),
                       include: x => x.Include(x => x.StudentClasses).Include(x => x.Schedules).ThenInclude(sc => sc.Attendances));

                    foreach(var cls in classes)
                    {

                    }
                    // _unitOfWork.GetRepository<WalletTransaction>().UpdateRange(transactions);
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Updating Transactions Got An Error: [{ex.Message}]";
            }
            return "Updating Transactions Success";
        }
    }
}
