using MagicLand_System.PayLoad.Response.Topics;

namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class ScheduleResWithTopic : ScheduleWithoutLectureResponse
    {
        public TopicWithSingleSession? Topic { get; set; } = new TopicWithSingleSession();
    }
}
