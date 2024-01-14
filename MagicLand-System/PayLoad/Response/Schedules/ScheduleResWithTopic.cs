using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class ScheduleResWithTopic : ScheduleResponse
    {
        public TopicResponse? Topic { get; set; } = new TopicResponse();
    }
}
