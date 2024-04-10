using MagicLand_System.Background.BackgroundServiceInterfaces;
using Quartz;

namespace MagicLand_System.Background.DailyJob
{
    [DisallowConcurrentExecution]
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
            _logger.LogInformation($"Daily Create Job Running At [{BackgoundTime.GetTime()}]");

            message += await _notificationBackgroundService.CreateNewNotificationInCondition();
            message += await _notificationBackgroundService.CreateNotificationForLastRegisterTime();
            message += await _notificationBackgroundService.CreateNotificationForRemindRegisterCourse();

            _logger.LogInformation($"Daily Create Job Completed At [{BackgoundTime.GetTime()}] With Message [{string.Join(", ", message)}]");
        }

    }
}
