using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System.Mappers.Custom
{
    public class CartItemCustomMapper
    {
        public static FavoriteItemResponse fromCartItemToFavoriteItemResponse(Course course, Guid itemId)
        {
            if(course == null || itemId == default)
            {
                return new FavoriteItemResponse();
            }

            var response = new FavoriteItemResponse
            {
                ItemId = itemId,
                Course = CourseCustomMapper.fromCourseToCourseResponse(course),
            };

            return response;
        }
        public static CartItemResponse fromCartItemToCartItemResponse(Guid cartItemId, ClassResExtraInfor cls, IEnumerable<Student> students)
        {
            CartItemResponse response = new CartItemResponse
            {
                ItemId = cartItemId,
                Students = students.Count() == 0
                ? new List<StudentResponse>()
                : students.Select(s => StudentCustomMapper.fromStudentToStudentResponse(s)).ToList(),
                Class = cls
            };

            return response;
        }
    }
}
