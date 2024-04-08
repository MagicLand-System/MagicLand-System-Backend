using MagicLand_System.Background.BackgroundServiceInterfaces;
using Quartz;

namespace MagicLand_System.Background.DailyJob
{
    [DisallowConcurrentExecution]
    public class DailyDeleteJob : IJob
    {
        private readonly ILogger<DailyDeleteJob> _logger;
        private readonly ITempEntityBackgroundService _tempEntityBackgroundService;
        public DailyDeleteJob(ILogger<DailyDeleteJob> logger, ITempEntityBackgroundService tempEntityBackgroundService)
        {
            _logger = logger;
            _tempEntityBackgroundService = tempEntityBackgroundService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string message = "";
            _logger.LogInformation($"Daily Update Job Running At [{DateTime.UtcNow}]");

            message += await _tempEntityBackgroundService.DeleteTempEntityByCondition();

            _logger.LogInformation($"Daily Update Job Completed At [{DateTime.UtcNow}] With Message [{string.Join(", ", message)}]");
        }

    }
}