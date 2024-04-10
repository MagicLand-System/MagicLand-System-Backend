using MagicLand_System.Background.DailyJob;
using Microsoft.Extensions.Options;
using Quartz;

namespace MagicLand_System.Background.BackgroundSetUp
{
    public class DailyUpdateJobSetUp : IConfigureOptions<QuartzOptions>
    {
        private readonly List<JobCronExpression> _cronExpressions;

        public DailyUpdateJobSetUp(IOptions<List<JobCronExpression>> cronExpressions)
        {
            _cronExpressions = cronExpressions.Value;
        }

        public void Configure(QuartzOptions options)
        {
            var jobKey = JobKey.Create(nameof(DailyUpdateJob));

            var cronExpression = _cronExpressions
           .FirstOrDefault(j => j.JobName == nameof(DailyUpdateJob))?
           .CronExpression ?? "0 0 0/1 ? * * *";

            options
           .AddJob<DailyUpdateJob>(jobBuilder => jobBuilder.WithIdentity(jobKey))
           .AddTrigger(trigger => trigger
           .ForJob(jobKey)
           .WithCronSchedule(cronExpression));
        }
    }
}
