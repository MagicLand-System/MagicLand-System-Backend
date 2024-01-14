using MagicLand_System.PayLoad.Response.Courses;

namespace MagicLand_System.PayLoad.Response.Carts
{
    public class FavoriteItemResponse
    {
        public Guid ItemId { get; set; }
        public CourseResponse? Course { get; set; } = new CourseResponse();
    }
}
