using MagicLand_System.Domain;
using MagicLand_System.Repository.Interfaces;
using Quartz;

namespace MagicLand_System.Services.System
{
    public class MyJob : IJob
    {
        private readonly IUnitOfWork<MagicLandContext> _unitOfWork;
        private readonly ILogger<MyJob> _logger;
        public string Expression { get; }

        public MyJob( string expression, ILogger<MyJob> logger, IUnitOfWork<MagicLandContext> unitOfWork)
        {
            Expression = expression;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {

            _logger.LogDebug("Job logic executed.");

            await Task.CompletedTask;
        }

    }
}
