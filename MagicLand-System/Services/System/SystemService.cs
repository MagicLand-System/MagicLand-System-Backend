using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Repository.Interfaces;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;

namespace MagicLand_System.Services.System
{
    //public class SystemService : BackgroundService
    //{
    //    private readonly ILogger<SystemService> _logger;
    //    private readonly IScheduler _scheduler;

    //    public SystemService(ILogger<SystemService> logger, IScheduler scheduler)
    //    {
    //        _logger = logger;
    //        _scheduler = scheduler;
    //    }

    //    public override Task StartAsync(CancellationToken cancellationToken)
    //    {
    //        _logger.LogInformation("System service is starting.");

    //        var job = JobBuilder.Create<DailyUpdateJob>()
    //            .WithIdentity("dailyJob", "group1")
    //            .Build();

    //        var trigger = TriggerBuilder.Create()
    //            .WithIdentity("dailyTrigger", "group1")
    //            .WithDailyTimeIntervalSchedule(s =>
    //                s.OnEveryDay()
    //                 .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0)) 
    //            )
    //            .Build();

    //        _scheduler.ScheduleJob(job, trigger);
    //        _scheduler.Start();

    //        return base.StartAsync(cancellationToken);
    //    }

    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        while (!stoppingToken.IsCancellationRequested)
    //        {
    //            _logger.LogInformation("Performing a background task...");


    //            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
    //        }
    //    }
    //}

    public class SystemService : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SystemService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider!.GetRequiredService(bundle!.JobDetail!.JobType!)! as IJob;
        }

        public void ReturnJob(IJob job)
        {
            throw new NotImplementedException();
        }
    }
}
