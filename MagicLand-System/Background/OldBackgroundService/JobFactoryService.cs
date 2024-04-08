using Quartz;
using Quartz.Spi;

namespace MagicLand_System.Background.OldBackgroundService
{
    //public class JobFactoryService : IJobFactory
    //{
    //    private readonly IServiceScopeFactory _serviceScopeFactory;

    //    public JobFactoryService(IServiceScopeFactory serviceScopeFactory)
    //    {
    //        _serviceScopeFactory = serviceScopeFactory;
    //    }

    //    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    //    {
    //        using (var scope = _serviceScopeFactory.CreateScope())
    //        {
    //            var job = scope?.ServiceProvider?.GetRequiredService(bundle.JobDetail!.JobType) as IJob;
    //            return job!;
    //        }
    //    }

    //    public void ReturnJob(IJob job)
    //    {

    //    }
    //}
}
