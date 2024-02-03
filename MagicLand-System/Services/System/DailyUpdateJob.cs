using MagicLand_System.Domain;
using MagicLand_System.Repository.Interfaces;
using Quartz;

namespace MagicLand_System.Services.System
{
    public class DailyUpdateJob : IJob
    {
        private readonly ILogger<DailyUpdateJob> _logger;
        private readonly IUnitOfWork<MagicLandContext> _unitOfWork;

        public DailyUpdateJob(ILogger<DailyUpdateJob> logger, IUnitOfWork<MagicLandContext> unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Executing the daily job...");

            // Example: Call a method in your service or perform database updates
            await YourDailyTaskMethod();

            _logger.LogInformation("Daily job completed.");
        }

        private async Task YourDailyTaskMethod()
        {
            // Your implementation
        }


    }
}
