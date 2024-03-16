using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace MagicLand_System.Background
{
    public class DailyUpdateJob : IJob
    {
        private readonly ILogger<DailyUpdateJob> _logger;
        private readonly IClassBackgroundService _classBackgroundService;
        private readonly ITransactionBackgroundService _transactionBackgroundService;
        private readonly INotificationBackgroundService _notificationBackgroundService;
        private readonly ITempEntityBackgroundService _tempEntityBackgroundService;
        public DailyUpdateJob(ILogger<DailyUpdateJob> logger, IClassBackgroundService classBackgroundService, ITransactionBackgroundService transactionBackgroundService, INotificationBackgroundService notificationBackgroundService, ITempEntityBackgroundService tempEntityBackgroundService)
        {
            _logger = logger;
            _classBackgroundService = classBackgroundService;
            _transactionBackgroundService = transactionBackgroundService;
            _notificationBackgroundService = notificationBackgroundService;
            _tempEntityBackgroundService = tempEntityBackgroundService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string message = "";
            _logger.LogInformation($"Daily Update Job Running At [{DateTime.Now}]");

            message += await _classBackgroundService.UpdateClassInTimeAsync();
            message += await _transactionBackgroundService.UpdateTransactionAfterTime();
            message += await _notificationBackgroundService.ModifyNotificationAfterTime();
            message += await _tempEntityBackgroundService.UpdateTempItemPrice();

            _logger.LogInformation($"Daily Update Job Completed At [{DateTime.Now}] With Message [{string.Join(", ", message)}]");
        }

    }
}
