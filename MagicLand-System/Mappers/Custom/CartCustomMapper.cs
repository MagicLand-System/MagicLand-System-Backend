using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.PayLoad.Response.Courses;

namespace MagicLand_System.Mappers.CustomMapper
{
    public class CartCustomMapper
    {
        public static FavoriteResponse fromCartToFavoriteResponse(Guid cartId, List<CartItem> cartItems, List<Course> courses)
        {
            if (cartItems == null || !courses.Any() || cartId == default)
            {
                return new FavoriteResponse();
            }
            var FavoriteItems = new List<CourseResponse>();

            foreach (var item in cartItems)
            {
                var favoriteItem = CourseCustomMapper.fromCourseToCourseResponse(courses.First(c => c.Id == item.CourseId));
                favoriteItem.IsInCart = true;
                favoriteItem.CartItemId = item.Id;

                FavoriteItems.Add(favoriteItem);
            }

            var response = new FavoriteResponse
            {
                CartId = cartId,
                FavoriteItems = FavoriteItems,
            };

            return response;
        }

        public static WishListResponse fromCartToWishListResponse(Cart cart, List<Student> students, List<ClassResExtraInfor> cls)
        {
            if (cart == null || cart.CartItems == null || cls == null)
            {
                return new WishListResponse();
            }

            // Leave Incase Error

            #region
            // Way 1
            //var cartResponses = cart.Carts.Select(cts =>
            //{
            //    var classEntity = cls.FirstOrDefault(cls => cls.Id == cts.ClassId)!;

            //    List<Student> studentsForCartItem = new List<Student>();
            //    foreach (var s in cts.CartItemRelations)
            //    {
            //        studentsForCartItem.Add(students.FirstOrDefault(stu => stu.Id == s.StudentId)!);
            //    }
            //    return fromCartItemToCartItemResponse(cts.Id, classEntity, studentsForCartItem);
            //}).ToList();

            // Way 2
            //CartResponse response = new CartResponse
            //{
            //    Id = cart.Id,
            //    CartItems = cart.Carts.Select(cts =>
            //    {
            //        var classEntity = cls.FirstOrDefault(cls => cls.Id == cts.ClassId)!;

            //        var studentsForCartItem = cts.CartItemRelations
            //            .Select(cir => students.FirstOrDefault(stu => stu.Id == cir.StudentId))
            //            .ToList();

            //        return fromCartItemToCartItemResponse(cts.Id, classEntity, studentsForCartItem);
            //    }).ToList()
            //};
            #endregion

            WishListResponse response = new WishListResponse
            {
                CartId = cart.Id,
                Items = cart.CartItems.Select(cts => CartItemCustomMapper.fromCartItemToCartItemResponse(
                cts.Id,
                cls.FirstOrDefault(cls => cls.ClassId == cts.ClassId)!,
                cts.StudentInCarts.Select(sic => students.FirstOrDefault(stu => stu.Id == sic.StudentId))!)).ToList()
            };

            return response;
        }

    }
}
