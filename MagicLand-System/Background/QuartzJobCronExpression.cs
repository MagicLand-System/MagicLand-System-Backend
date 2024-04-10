namespace MagicLand_System.Background
{
    public class QuartzJobCronExpression
    {
        public QuartzJobCronExpression()
        {

        }

        public List<JobCronExpression>? QuartzJobs { get; set; }
    }

    public class JobCronExpression
    {
        public string? JobName { get; set; }
        public string? CronExpression { get; set; }
    }
}
