using MagicLand_System.Background.DailyJob;
using Microsoft.Extensions.Options;
using Quartz;

namespace MagicLand_System.Background.BackgroundSetUp
{
    public class DailyCreateJobSetUp : IConfigureOptions<QuartzOptions>
    {
        private readonly List<JobCronExpression> _cronExpressions;

        public DailyCreateJobSetUp(IOptions<List<JobCronExpression>> cronExpressions)
        {
            _cronExpressions = cronExpressions.Value;
        }
        public void Configure(QuartzOptions options)
        {
            var jobKey = JobKey.Create(nameof(DailyCreateJob));

            var cronExpression = _cronExpressions
           .FirstOrDefault(j => j.JobName == nameof(DailyCreateJob))?
           .CronExpression ?? "0 0 0/1 ? * * *";

            //for local
            options
            .AddJob<DailyCreateJob>(jobBuilder => jobBuilder.WithIdentity(jobKey))
            .AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithCronSchedule(cronExpression));
            //for server
            // options
            //.AddJob<DailyCreateJob>(jobBuilder => jobBuilder.WithIdentity(jobKey))
            //.AddTrigger(trigger => trigger
            //.ForJob(jobKey)
            //.WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(30).RepeatForever()).StartNow().Build());

        }
    }
}
