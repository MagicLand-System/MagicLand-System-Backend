using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
using MagicLand_System.Repository.Interfaces;

namespace MagicLand_System.Background.BackgroundServiceImplements
{
    public class TransactionBackgroundService : ITransactionBackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TransactionBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<string> UpdateTransactionAfterTime()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentTime = BackgoundTime.GetTime();

                    var transactions = await _unitOfWork.GetRepository<WalletTransaction>()
                      .GetListAsync(predicate: x => x.Status == TransactionStatusEnum.Processing.ToString());

                    foreach (var trans in transactions)
                    {
                        if (currentTime.Day - trans.CreateTime.Day >= 15)
                        {
                            trans.Status = TransactionStatusEnum.Failed.ToString();
                            trans.UpdateTime = currentTime;
                        }
                    }

                    _unitOfWork.GetRepository<WalletTransaction>().UpdateRange(transactions);
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
