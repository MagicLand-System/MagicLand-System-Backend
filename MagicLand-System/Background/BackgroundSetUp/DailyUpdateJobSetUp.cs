using MagicLand_System.Background.DailyJob;
using Microsoft.Extensions.Options;
using Quartz;

namespace MagicLand_System.Background.BackgroundSetUp
{
    public class DailyUpdateJobSetUp : IConfigureOptions<QuartzOptions>
    {
        private readonly QuartzJobCronExpression _quartzJobCronExpression;

        public DailyUpdateJobSetUp(IOptions<QuartzJobCronExpression> quartzJobSettings)
        {
            _quartzJobCronExpression = quartzJobSettings.Value;
        }

        public void Configure(QuartzOptions options)
        {
            var jobKey = JobKey.Create(nameof(DailyUpdateJob));

            var cronExpression = _quartzJobCronExpression.QuartzJobs != null && _quartzJobCronExpression.QuartzJobs.Any()
              ? _quartzJobCronExpression.QuartzJobs.FirstOrDefault(job => job.JobName == nameof(DailyCreateJob))?.CronExpression
              : "0 0 0/1 ? * * *";

            options
           .AddJob<DailyUpdateJob>(jobBuilder => jobBuilder.WithIdentity(jobKey))
           .AddTrigger(trigger => trigger
           .ForJob(jobKey)
           .WithCronSchedule("0,5 0,15 7,9,12,14,16,19 ? * * *"));
        }
    }
}
