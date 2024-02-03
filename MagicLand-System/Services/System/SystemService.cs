using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Repository.Interfaces;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace MagicLand_System.Services.System
{
    public class SystemService : BackgroundService
    {
        private readonly ILogger<SystemService> _logger;
        private readonly IScheduler _scheduler;

        public SystemService(ILogger<SystemService> logger, IScheduler scheduler)
        {
            _logger = logger;
            _scheduler = scheduler;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("System service is starting.");

            var job = JobBuilder.Create<DailyUpdateJob>()
                .WithIdentity("dailyJob", "group1")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("dailyTrigger", "group1")
                .WithDailyTimeIntervalSchedule(s =>
                    s.OnEveryDay()
                     .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))  // Set the start time to midnight
                )
                .Build();

            _scheduler.ScheduleJob(job, trigger);
            _scheduler.Start();

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Performing a background task...");

                // Your logic goes here

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
