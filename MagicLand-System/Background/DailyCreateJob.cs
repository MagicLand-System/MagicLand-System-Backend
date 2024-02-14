using MagicLand_System.Background.BackgroundServiceInterfaces;
using Quartz;

namespace MagicLand_System.Background
{
    public class DailyCreateJob : IJob
    {
        private readonly ILogger<DailyCreateJob> _logger;
        private readonly INotificationBackgroundService _notificationBackgroundService;

        public DailyCreateJob(ILogger<DailyCreateJob> logger, INotificationBackgroundService notificationBackgroundService)
        {
            _logger = logger;
            _notificationBackgroundService = notificationBackgroundService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string message = "";
            _logger.LogInformation($"Daily Create Job Running At [{DateTime.Now}]");

            message += await _notificationBackgroundService.CreateNewNotificationInCondition();

            _logger.LogInformation($"Daily Create Job Completed At [{DateTime.Now}] With Message [{string.Join(", ", message)}]");
        }

    }
}
