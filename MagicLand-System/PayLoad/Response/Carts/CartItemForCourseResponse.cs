using MagicLand_System.PayLoad.Response.Courses;
using MagicLand_System.PayLoad.Response.Schedules;

namespace MagicLand_System.PayLoad.Response.Carts
{
    public class CartItemForCourseResponse : WishListItemResponse
    {
        public required string MainDescription { get; set; }
        public List<SubDescriptionTitleResponse>? SubDescriptionTitle { get; set; }  = new List<SubDescriptionTitleResponse>();
        public List<string>? CoursePrerequisites { get; set; } = new List<string>();
        public List<OpeningScheduleResponse>? Schedules { get; set; } = new List<OpeningScheduleResponse>();
    }
}
