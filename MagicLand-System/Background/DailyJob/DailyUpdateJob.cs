using MagicLand_System.Background.BackgroundServiceInterfaces;
using Quartz;

namespace MagicLand_System.Background.DailyJob
{
    [DisallowConcurrentExecution]
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
            _logger.LogInformation($"Daily Update Job Running At [{DateTime.UtcNow}]");

            message += await _classBackgroundService.UpdateClassInTimeAsync();
            message += await _transactionBackgroundService.UpdateTransactionAfterTime();
            message += await _notificationBackgroundService.ModifyNotificationAfterTime();

            _logger.LogInformation($"Daily Update Job Completed At [{DateTime.UtcNow}] With Message [{string.Join(", ", message)}]");
        }

    }
}
