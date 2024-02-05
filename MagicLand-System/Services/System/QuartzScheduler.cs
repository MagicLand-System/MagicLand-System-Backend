using Quartz.Impl;
using Quartz;
using Quartz.Spi;

namespace MagicLand_System.Services.System
{
    public class QuartzScheduler : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<MyJob> _myJobs;

        public QuartzScheduler(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, IEnumerable<MyJob> myJobs)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _myJobs = myJobs;
        }

        public IScheduler _scheduler { get; set; }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Running Inside");
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            _scheduler.JobFactory = _jobFactory;

            foreach(var myJob in _myJobs)
            {
                var job = CreateJob(myJob);
                var trigger = CreateTrigger(myJob);
                await _scheduler.ScheduleJob(job, trigger, cancellationToken);

            }

            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Stop!");
            await _scheduler!.Shutdown(cancellationToken);
        }

        private static IJobDetail CreateJob(MyJob job)
        {
            var type = job.GetType();
            return JobBuilder.Create(type).WithIdentity(type.FullName!).WithDescription(type.Name).Build();
        }

        private static ITrigger CreateTrigger(MyJob job)
        {
            return TriggerBuilder.Create().WithIdentity($"{job.GetType().FullName}.trigger").WithCronSchedule(job.Expression).WithDescription(job.Expression).Build();
        }

    }
}
