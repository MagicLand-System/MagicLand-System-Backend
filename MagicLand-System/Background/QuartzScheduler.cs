using Quartz;
using Quartz.Spi;
using static Quartz.Logging.OperationName;

namespace MagicLand_System.Background
{
    public class QuartzScheduler : IHostedService
    {
        private readonly ILogger<QuartzScheduler> _logger;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<Job> _jobs;
        public IScheduler? _scheduler { get; set; }

        public QuartzScheduler(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, IEnumerable<Job> Jobs, ILogger<QuartzScheduler> logger)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _jobs = Jobs;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            _scheduler.JobFactory = _jobFactory;

            foreach (var job in _jobs)
            {
                var jobCreated = CreateJob(job);
                var triggerCreated = CreateTrigger(job);
                await _scheduler.ScheduleJob(jobCreated, triggerCreated, cancellationToken);

                _logger.LogInformation($"Scheduling [{job._type.Name}] At [{DateTime.Now}]");
            }

            _logger.LogInformation($"Start All Job!");
            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stop All Job!");
            await _scheduler!.Shutdown(cancellationToken);
        }

        private static IJobDetail CreateJob(Job job)
        {
            var type = job._type;
            return JobBuilder.Create(type).WithIdentity(type.FullName!).WithDescription(type.Name).Build();
        }

        private static ITrigger CreateTrigger(Job job)
        {
            string identity = job._type! + ".trigger";
            string expression = job._expression;

            var trigger = TriggerBuilder.Create().WithIdentity(identity).WithCronSchedule(expression).WithDescription(expression).Build();
            return trigger;
        }

    }
}
