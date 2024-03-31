using MagicLand_System.PayLoad.Response.Courses;

namespace MagicLand_System.PayLoad.Response.Carts
{
    public class FavoriteResponse
    {
        public Guid CartId { get; set; }
        public List<CourseResponse> FavoriteItems { get; set; } = new List<CourseResponse>();
    }
}
