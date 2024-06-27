using Microsoft.Extensions.Logging;
using Quartz;


namespace MagicLand_System_Background
{
    [DisallowConcurrentExecution]
    public class LoggingBackgroundJob : IJob
    {
        private readonly ILogger<LoggingBackgroundJob> _logger;

        public LoggingBackgroundJob(ILogger<LoggingBackgroundJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Running at {DateTime.UtcNow}");
            return Task.CompletedTask;
        }
    }
}
