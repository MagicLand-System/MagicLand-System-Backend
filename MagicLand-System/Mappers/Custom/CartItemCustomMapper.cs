using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Carts.GeneralCart;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System.Mappers.Custom
{
    public class CartItemCustomMapper
    {
        public static CartItemResponse fromCartItemToCartItemResponse(Course course, Class? cls, Guid itemId)
        {
            if (course == null || itemId == default)
            {
                return new CartItemResponse { ItemType = "Unknow"};
            }

            var response = new CartItemResponse
            {
                CartItemId = itemId,
                ItemId = cls != null ? cls.Id : course.Id,
                ItemType = cls != null ? "CLASS" : "COURSE",
                Name = cls != null ? cls.ClassCode : course.Name,
                Code = cls != null ? cls.ClassCode : course.Syllabus!.SubjectCode,
                Subject = course.SubjectName,
                MinYearOldStudent = course.MinYearOldsStudent!.Value,
                MaxYearOldStudent = course.MinYearOldsStudent!.Value,
                Image = cls != null ? cls.Image : course.Image    
            };

            return response;
        }
        public static FavoriteItemResponse fromCartItemToFavoriteItemResponse(Course course, Guid itemId)
        {
            if(course == null || itemId == default)
            {
                return new FavoriteItemResponse();
            }

            var response = new FavoriteItemResponse
            {
                Course = CourseCustomMapper.fromCourseToCourseResponse(course),
            };

            return response;
        }
        public static WishListItemResponse fromCartItemToCartItemResponse(Guid cartItemId, ClassResExtraInfor cls, IEnumerable<Student> students)
        {
            WishListItemResponse response = new WishListItemResponse
            {
                CartItemId = cartItemId,
                Students = students.Count() == 0
                ? new List<StudentResponse>()
                : students.Select(s => StudentCustomMapper.fromStudentToStudentResponse(s)).ToList(),
                Class = cls
            };

            return response;
        }
    }
}
