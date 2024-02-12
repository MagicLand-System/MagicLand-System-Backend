using MagicLand_System.Background.BackgroundServiceInterfaces;
using Quartz;

namespace MagicLand_System.Background
{
    public class DailyCreateJob : IJob
    {
        private readonly ILogger<DailyCreateJob> _logger;
        private readonly IClassBackroundService _classBackgroundService;
        private readonly ITransactionBackgroundService _transactionBackgroundService;

        public DailyCreateJob(ILogger<DailyCreateJob> logger, IClassBackroundService classBackgroundService, ITransactionBackgroundService transactionBackgroundService)
        {
            _logger = logger;
            _classBackgroundService = classBackgroundService;
            _transactionBackgroundService = transactionBackgroundService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string message = "";
            _logger.LogInformation($"Daily Update Job Running At [{DateTime.Now}]");

            message += await _classBackgroundService.UpdateClassInTimeAsync();
            message += await _transactionBackgroundService.UpdateTransactionAfterTime();

            _logger.LogInformation($"Daily Completed At [{DateTime.Now}] With Message [{string.Join(", ", message)}]");
        }

    }
}
