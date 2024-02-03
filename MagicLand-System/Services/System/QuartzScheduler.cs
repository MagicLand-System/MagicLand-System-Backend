using Quartz.Impl;
using Quartz;

namespace MagicLand_System.Services.System
{
    public class QuartzScheduler
    {
        public static async Task Start()
        {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            IJobDetail job = JobBuilder.Create<DailyUpdateJob>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                (s =>
                    s.OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(12, 0)) 
                )
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
        }
    }
}
