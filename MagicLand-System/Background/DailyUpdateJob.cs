using MagicLand_System.Background.BackgroundServiceInterfaces;
using Quartz;

namespace MagicLand_System.Background
{
    public class DailyUpdateJob : IJob
    {
        private readonly ILogger<DailyUpdateJob> _logger;
        private readonly IClassBackroundService _classService;

        public DailyUpdateJob(IClassBackroundService classService, ILogger<DailyUpdateJob> logger)
        {
            _classService = classService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Daily Update Job Running At [{DateTime.Now}]");

            var s = _classService.UpdateStatusClassInTimeAsync();
        }


    }
}
