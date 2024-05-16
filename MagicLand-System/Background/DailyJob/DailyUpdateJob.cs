using MagicLand_System.Background.BackgroundServiceInterfaces;
using Quartz;

namespace MagicLand_System.Background.DailyJob
{
    public class DailyUpdateJob : IJob
    {
        private readonly ILogger<DailyUpdateJob> _logger;
        private readonly IClassBackgroundService _classBackgroundService;
        private readonly ITransactionBackgroundService _transactionBackgroundService;
        public DailyUpdateJob(ILogger<DailyUpdateJob> logger, IClassBackgroundService classBackgroundService, ITransactionBackgroundService transactionBackgroundService)
        {
            _logger = logger;
            _classBackgroundService = classBackgroundService;
            _transactionBackgroundService = transactionBackgroundService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string message = "";
            _logger.LogInformation($"Daily Update Job Running At [{BackgoundTime.GetTime()}]");

            message += await _classBackgroundService.UpdateClassInTimeAsync();
            message += await _transactionBackgroundService.UpdateTransactionAfterTime();

            _logger.LogInformation($"Daily Update Job Completed At [{BackgoundTime.GetTime()}] With Message [{string.Join(", ", message)}]");
        }

    }
}
