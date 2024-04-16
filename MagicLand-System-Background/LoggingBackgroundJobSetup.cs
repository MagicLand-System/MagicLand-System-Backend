using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System_Background
{
    public class LoggingBackgroundJobSetup : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            var jobKey = JobKey.Create(nameof(LoggingBackgroundJob));
            options
            .AddJob<LoggingBackgroundJob>(jobBuilder => jobBuilder.WithIdentity(jobKey))
            .AddTrigger(trigger =>
            trigger.ForJob(jobKey)
            .WithSimpleSchedule(schedule =>
            schedule.WithIntervalInSeconds(30).RepeatForever()));
        }
    }
}
