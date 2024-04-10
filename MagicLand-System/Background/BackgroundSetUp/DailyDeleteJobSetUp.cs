using MagicLand_System.Background.DailyJob;
using Microsoft.Extensions.Options;
using Quartz;

namespace MagicLand_System.Background.BackgroundSetUp
{
    public class DailyDeleteJobSetUp : IConfigureOptions<QuartzOptions>
    {
        private readonly List<JobCronExpression> _cronExpressions;

        public DailyDeleteJobSetUp(IOptions<List<JobCronExpression>> cronExpressions)
        {
            _cronExpressions = cronExpressions.Value;
        }

        public void Configure(QuartzOptions options)
        {
            var jobKey = JobKey.Create(nameof(DailyDeleteJob));

            var cronExpression = _cronExpressions
            .FirstOrDefault(j => j.JobName == nameof(DailyDeleteJob))?
            .CronExpression ?? "0 0 0/1 ? * * *";


             options
            .AddJob<DailyDeleteJob>(jobBuilder => jobBuilder.WithIdentity(jobKey))
            .AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithCronSchedule(cronExpression));
        }
    }
}
