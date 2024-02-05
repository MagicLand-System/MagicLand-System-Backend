using MagicLand_System.Domain;
using MagicLand_System.Repository.Interfaces;
using Quartz;

namespace MagicLand_System.Services.System
{
    public class DailyUpdateJob : IJob
    {
        private readonly IUnitOfWork<MagicLandContext> _unitOfWork;

        public DailyUpdateJob(ILogger<DailyUpdateJob> logger, IUnitOfWork<MagicLandContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task Execute(IJobExecutionContext context)
        {

            // Example: Call a method in your service or perform database updates
            await YourDailyTaskMethod();

        }

        private async Task YourDailyTaskMethod()
        {
            Console.WriteLine("Help");
        }


    }
}
