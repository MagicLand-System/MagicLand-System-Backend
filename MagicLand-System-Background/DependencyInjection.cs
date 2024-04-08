using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace MagicLand_System_Background
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();

            });

            services.AddQuartzHostedService(options => {
                options.WaitForJobsToComplete = true;
            });

            services.ConfigureOptions<LoggingBackgroundJobSetup>();
        }
    }
}
