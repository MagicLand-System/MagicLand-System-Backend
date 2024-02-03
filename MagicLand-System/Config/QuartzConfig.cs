using Quartz.Impl;
using Quartz;

namespace MagicLand_System.Config
{
    public class QuartzConfig
    {
        public IScheduler Scheduler { get; }

        public QuartzConfig()
        {
            var schedulerFactory = new StdSchedulerFactory();
            Scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
        }
    }
}
