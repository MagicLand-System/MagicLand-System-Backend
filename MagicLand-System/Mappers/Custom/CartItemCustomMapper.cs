using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Students;

namespace MagicLand_System.Mappers.Custom
{
    public class CartItemCustomMapper
    {
        public static CartItemForClassResponse fromCartItemToCartItemForClassResponse(Guid cartItemId, ClassResExtraInfor cls, Course course, List<Student> students)
        {
            if (cls == null || cartItemId == default || !students.Any() || course == null)
            {
                return default!;
            }

            var response = new CartItemForClassResponse
            {
               CartItemId = cartItemId,
               ItemType = "Class",
               ItemId = cls.ClassId,
               Name = cls.ClassName,
               Code = cls.ClassCode,
               Subject = cls.ClassSubject,
               Price = cls.CoursePrice,
               MinYearOldStudent = course.MinYearOldsStudent!.Value,
               MaxYearOldStudent = course.MaxYearOldsStudent!.Value,
               Image = cls.Image,
               StartDate = cls.StartDate,
               EndDate = cls.EndDate,
               Method = cls.Method!,
               LimitNumberStudent = cls.LimitNumberStudent,
               LeastNumberStudent = cls.LeastNumberStudent,
               Video = cls.Video!,
               Students = students.Select(s => StudentCustomMapper.fromStudentToStudentResponse(s)).ToList(),
               Lecture = cls.Lecture!,
               Schedules = cls.Schedules!,
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
                ItemId = itemId,
                Course = CourseCustomMapper.fromCourseToCourseResponse(course),
            };

            return response;
        }
        //public static CartItemResponse fromCartItemToCartItemResponse(Guid cartItemId, ClassResExtraInfor cls, IEnumerable<Student> students)
        //{
        //    CartItemResponse response = new CartItemResponse
        //    {
        //        CartItemId = cartItemId,
        //        Students = students.Count() == 0
        //        ? new List<StudentResponse>()
        //        : students.Select(s => StudentCustomMapper.fromStudentToStudentResponse(s)).ToList(),
        //        Class = cls
        //    };

        //    return response;
        //}
    }
}
