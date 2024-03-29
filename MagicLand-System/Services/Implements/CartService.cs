using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.Mappers.CustomMapper;
using MagicLand_System.PayLoad.Response.Carts;
using MagicLand_System.PayLoad.Response.Carts.GeneralCart;
using MagicLand_System.PayLoad.Response.Classes;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class CartService : BaseService<CartService>, ICartService
    {
        public CartService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<CartService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        #region for inform response
        //public async Task<CartForCourseResponse> AddCourseFavoriteOffCurrentParentAsync(Guid courseId)
        //{
        //    var currentParentCart = await FetchCurrentParentCart();

        //    var favoriteResponse = new CartForCourseResponse();
        //    try
        //    {
        //        if (currentParentCart.CartItems.Count() > 0 && currentParentCart.CartItems.Any(x => x.CourseId == courseId))
        //        {
        //            throw new BadHttpRequestException($"Id [{courseId}] Của Khóa Đã Có Trong Danh Sách Yêu Thích", StatusCodes.Status400BadRequest);
        //        }
        //        else
        //        {
        //            var newItem = new CartItem
        //            {
        //                Id = new Guid(),
        //                CartId = currentParentCart.Id,
        //                CourseId = courseId,
        //                DateCreated = DateTime.Now,
        //            };

        //            await _unitOfWork.GetRepository<CartItem>().InsertAsync(newItem);

        //            favoriteResponse = await _unitOfWork.CommitAsync() > 0
        //                 ? await GetDetailCurrentParrentFavorite()
        //                 : throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh", StatusCodes.Status500InternalServerError);
        //        }

        //        return favoriteResponse;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new BadHttpRequestException(ex.InnerException != null ? ex.InnerException.Message : ex.Message, StatusCodes.Status500InternalServerError);
        //    }
        //}


        //public async Task<CartForCourseResponse> GetDetailCurrentParrentFavorite()
        //{
        //    try
        //    {
        //        var currentParrentCart = await FetchCurrentParentCart();

        //        if (currentParrentCart != null && currentParrentCart.CartItems.Count() > 0)
        //        {
        //            var items = new List<CartItemForCourseResponse>();

        //            foreach (var item in currentParrentCart.CartItems)
        //            {
        //                if (item.CourseId == default)
        //                {
        //                    continue;
        //                }
        //                List<string> coursePrer = new List<string>();

        //                var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
        //                    predicate: c => c.Id == item.CourseId,
        //                    include: x => x.Include(x => x.SubDescriptionTitles).ThenInclude(sdt => sdt.SubDescriptionContents)
        //                    .Include(x => x.Syllabus!.SyllabusPrerequisites!));

        //                foreach (var id in course.Syllabus!.SyllabusPrerequisites!.Select(sp => sp.PrerequisiteSyllabusId))
        //                {
        //                    coursePrer.Add(await _unitOfWork.GetRepository<Syllabus>().SingleOrDefaultAsync(
        //                    selector: x => x.Course!.Name!,
        //                    predicate: c => c.Id == id,
        //                    include: x => x.Include(x => x.Course!)));
        //                }

        //                var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
        //                    predicate: c => c.CourseId == item.CourseId,
        //                   include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)
        //                   .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Room!));

        //                items.Add(CartItemCustomMapper.fromCartItemToCartItemForCourseResponse(item.Id, classes.ToList(), course, coursePrer));
        //            }

        //            return new CartForCourseResponse
        //            {
        //                CartId = currentParrentCart.Id,
        //                Items = items,
        //            };
        //        }

        //        return new CartForCourseResponse { CartId = currentParrentCart != null ? currentParrentCart.Id : default };

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}


        #endregion
        public async Task<bool> AddCourseFavoriteOffCurrentParentAsync(Guid courseId)
        {
            var currentParentCart = await FetchCurrentParentCart();

            //var favoriteResponse = new FavoriteResponse();
            //string message = string.Empty;
            bool result = true;

            var courseName = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                    selector: x => x.Name,
                    predicate: x => x.Id == courseId);
            try
            {
                if (currentParentCart.CartItems.Count() > 0 && currentParentCart.CartItems.Any(x => x.CourseId == courseId))
                {
                    return false;
                    //throw new BadHttpRequestException($"Id [{courseId}] Của Khóa Đã Có Trong Danh Sách Yêu Thích", StatusCodes.Status400BadRequest);
                    //return $"Khóa Học [{courseName}] Đã Có Trong Danh Sách Quan Tâm";
                }
                else
                {
                    var newItem = new CartItem
                    {
                        Id = new Guid(),
                        CartId = currentParentCart.Id,
                        CourseId = courseId,
                        DateCreated = DateTime.Now,
                    };

                    await _unitOfWork.GetRepository<CartItem>().InsertAsync(newItem);

                    //favoriteResponse = await _unitOfWork.CommitAsync() > 0
                    //     ? await GetDetailCurrentParrentFavorite()
                    //     : throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh", StatusCodes.Status500InternalServerError);

                    //message = await _unitOfWork.CommitAsync() > 0
                    //        ? $"Bạn Đã Quan Tâm Khóa Học [{courseName}]"
                    //        : $"Quan Tâm Khóa Học [{courseName}] Thất Bại, Vui Lòng Chờ Hệ Thống Sử Lý Và Thử Lại Sau";

                    result = await _unitOfWork.CommitAsync() > 0;

                }
                return result;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.InnerException != null ? ex.InnerException.Message : ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<bool> ModifyCartOffCurrentParentAsync(List<Guid> studentIds, Guid classId)
        {
            var currentParentCart = await FetchCurrentParentCart();

            //string message = string.Empty;
            try
            {
                var classCode = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                    selector: x => x.ClassCode,
                    predicate: x => x.Id == classId);

                if (currentParentCart.CartItems.Count() > 0 && currentParentCart.CartItems.Any(x => x.ClassId == classId))
                {
                    var currentCartItem = currentParentCart.CartItems.SingleOrDefault(x => x.ClassId == classId);

                    if (studentIds.Count() == 0 && currentCartItem!.StudentInCarts.Count() == 0)
                    {
                        //throw new BadHttpRequestException($"Id [{classId}] Của Lớp Đã Có Trong Giỏ Hàng", StatusCodes.Status400BadRequest);
                        //return $"[{classCode}] Đã Có Trong Giỏ Hàng";
                        return false;
                    }

                    if (currentCartItem!.StudentInCarts.Select(sic => sic.StudentId).ToList().SequenceEqual(studentIds))
                    {
                        var studentName = new List<string>();
                        foreach (Guid id in studentIds)
                        {
                            var name = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(
                                selector: x => x.FullName,
                                predicate: x => x.Id == id);

                            studentName.Add(name!);
                        }

                        //throw new BadHttpRequestException($"Bạn Đã Có Bé [{string.Join(", ", studentName)}] Trong Lớp [{classCode}] Ở Giỏ Hàng", StatusCodes.Status400BadRequest);
                        //return $"Bạn Đã Có Bé [{string.Join(", ", studentName)}] Trong Lớp [{classCode}] Ở Giỏ Hàng";
                        return false;
                    }

                    _unitOfWork.GetRepository<StudentInCart>().DeleteRangeAsync(currentCartItem!.StudentInCarts);

                    if (studentIds.Count() > 0)
                    {
                        await _unitOfWork.GetRepository<StudentInCart>().InsertRangeAsync
                       (
                            RenderStudentInClass(studentIds, currentCartItem)
                       );
                    }

                    //message = await _unitOfWork.CommitAsync() > 0
                    //    ? await GetDetailCurrentParrentCart()
                    //    : throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh", StatusCodes.Status500InternalServerError);

                    //message = await _unitOfWork.CommitAsync() > 0
                    //? $"Thêm Thành Công Lớp [{classCode}] Vào Giỏ Hàng"
                    //: $"Thêm Thất Bại Lớp [{classCode}] Vào Giỏ Hàng, Vui Lòng Chờ Hệ Thống Sử Lý Và Thử Lại Sau";

                    return await _unitOfWork.CommitAsync() > 0;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        Id = new Guid(),
                        CartId = currentParentCart.Id,
                        ClassId = classId,
                        DateCreated = DateTime.Now,
                    };

                    await _unitOfWork.GetRepository<CartItem>().InsertAsync(newItem);

                    if (studentIds.Count > 0)
                    {
                        await _unitOfWork.GetRepository<StudentInCart>().InsertRangeAsync
                        (
                         RenderStudentInClass(studentIds, newItem)
                        );
                    }

                    //message = await _unitOfWork.CommitAsync() > 0
                    //     ? await GetDetailCurrentParrentCart()
                    //     : throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh", StatusCodes.Status500InternalServerError);

                    //message = await _unitOfWork.CommitAsync() > 0
                    //? $"Thêm Thành Công Lớp [{classCode}] Vào Giỏ Hàng"
                    //: $"Thêm Thất Bại Lớp [{classCode}] Vào Giỏ Hàng, Vui Lòng Chờ Hệ Thống Sử Lý Và Thử Lại Sau";

                    return await _unitOfWork.CommitAsync() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.InnerException != null ? ex.InnerException.Message : ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<WishListResponse> GetDetailCurrentParrentCart()
        {
            try
            {
                var currentParrentCart = await FetchCurrentParentCart();
                var classes = new List<ClassResExtraInfor>();
                var students = new List<Student>();

                if (currentParrentCart != null && currentParrentCart.CartItems.Count() > 0)
                {
                    foreach (var item in currentParrentCart.CartItems)
                    {
                        if (item.CourseId != default)
                        {
                            continue;
                        }

                        var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                            predicate: x => x.Id == item.ClassId,
                            include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Slot)!
                           .Include(x => x.Lecture)
                           .Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(s => s.Room)!);

                        cls.Course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                            predicate: x => x.Id == cls.CourseId,
                            include: x => x.Include(x => x.Syllabus!));

                        var response = _mapper.Map<ClassResExtraInfor>(cls);
                        response.CoursePrice = await GetDynamicPrice(cls.Id, true);

                        classes.Add(response);


                        foreach (var task in item.StudentInCarts
                        .Where(studentInCart => studentInCart != null)
                        .Select(async studentInCart => await _unitOfWork.GetRepository<Student>()
                        .SingleOrDefaultAsync(predicate: c => c.Id == studentInCart.StudentId)))
                        {
                            var student = await task;
                            students.Add(student);
                        }
                    }

                    #region
                    //Leave InCase Using Back

                    //foreach (var cts in cart.Carts)
                    //{
                    //    var classEntity = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                    //        predicate: x => x.Id == cts.ClassId,
                    //        include: x => x.Include(x => x.User).Include(x => x.Address)!
                    //    );

                    //    var classResponse = _mapper.Map<ClassResponse>(classEntity);
                    //    classes.Add(classResponse);
                    //}
                    //var classes = await Task.WhenAll(cart.Carts.Select(async cts =>
                    //{
                    //    var classEntity = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == cts.ClassId,
                    //        include: x => x.Include(x => x.User).ThenInclude(u => u.Address).Include(x => x.Address!));

                    //    return _mapper.Map<ClassResponse>(classEntity);
                    //}));

                    //var students = new List<Student>();

                    //foreach (var cartItemRelation in cart.Carts.SelectMany(c => c.CartItemRelations ?? Enumerable.Empty<CartItemRelation>()))
                    //{
                    //    if (cartItemRelation == null)
                    //    {
                    //        continue;
                    //    }

                    //    var student = await _unitOfWork.GetRepository<Student>()
                    //        .SingleOrDefaultAsync(predicate: c => c.Id == cartItemRelation.StudentId);

                    //    students.Add(student);
                    //}
                    #endregion
                    return CartCustomMapper.fromCartToWishListResponse(currentParrentCart, students, classes);
                };


                return new WishListResponse { CartId = currentParrentCart != null ? currentParrentCart.Id : default };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<FavoriteResponse> GetDetailCurrentParrentFavorite()
        {
            try
            {
                var currentParrentCart = await FetchCurrentParentCart();

                if (currentParrentCart != null && currentParrentCart.CartItems.Count() > 0)
                {
                    var courses = new List<Course>();

                    var itemFavorites = currentParrentCart.CartItems.Where(ci => ci.CourseId != default).ToList();

                    foreach (var item in itemFavorites)
                    {
                        var course = await _unitOfWork.GetRepository<Course>()
                            .SingleOrDefaultAsync(predicate: c => c.Id == item.CourseId, include: x => x
                            .Include(x => x.SubDescriptionTitles).ThenInclude(sdt => sdt.SubDescriptionContents));
                        course.Syllabus = await _unitOfWork.GetRepository<Syllabus>()
                            .SingleOrDefaultAsync(predicate: x => x.Id == course.SyllabusId);
                        course.Classes = await _unitOfWork.GetRepository<Class>()
                            .GetListAsync(predicate: x => x.CourseId == course.Id);

                        courses.Add(course);

                    }
                    var response = CartCustomMapper.fromCartToFavoriteResponse(currentParrentCart.Id, itemFavorites, courses);
                    foreach (var fv in response.FavoriteItems)
                    {
                        fv.Price = await GetDynamicPrice(fv.CourseId, false);
                    }

                    return response;
                }

                return new FavoriteResponse { CartId = currentParrentCart != null ? currentParrentCart.Id : default };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteItemInCartOfCurrentParentAsync(List<Guid> itemIds)
        {
            try
            {
                var currentParentCart = await FetchCurrentParentCart();

                var cartItemDeleteList = new List<CartItem>();
                foreach (var id in itemIds)
                {
                    var cartItemDelete = currentParentCart.CartItems.SingleOrDefault(x => x.Id == id);
                    if (cartItemDelete == null)
                    {
                        throw new BadHttpRequestException($"Item Id [{id}] Của Giỏ Hàng Không Tồn Tại", StatusCodes.Status500InternalServerError);
                    }

                    cartItemDeleteList.Add(cartItemDelete);
                }

                _unitOfWork.GetRepository<CartItem>().DeleteRangeAsync(cartItemDeleteList);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<Cart> FetchCurrentParentCart()
        {
            return await _unitOfWork.GetRepository<Cart>().SingleOrDefaultAsync(
                predicate: x => x.UserId == GetUserIdFromJwt(),
                include: x => x.Include(x => x.CartItems.OrderByDescending(ci => ci.DateCreated)).ThenInclude(cts => cts.StudentInCarts));
        }

        private List<StudentInCart> RenderStudentInClass(List<Guid> studentIds, CartItem cartItem)
        {
            return studentIds.Select(s => new StudentInCart
            {
                Id = new Guid(),
                CartItemId = cartItem.Id,
                StudentId = s
            }).ToList();
        }

        public async Task<CartResponse> GetAllItemsInCartAsync()
        {
            try
            {
                var currentParrentCart = await FetchCurrentParentCart();

                if (currentParrentCart != null && currentParrentCart.CartItems.Count() > 0)
                {
                    var response = new CartResponse { CartId = currentParrentCart.Id };

                    await GenrateCartItem(currentParrentCart, response);

                    return response;
                }

                return new CartResponse { CartId = currentParrentCart != null ? currentParrentCart.Id : default };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task GenrateCartItem(Cart currentParrentCart, CartResponse response)
        {
            foreach (var item in currentParrentCart.CartItems)
            {
                var itemResponse = new CartItemResponse { ItemType = "" };

                if (item.CourseId == default)
                {
                    var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(
                        predicate: c => c.Id == item.ClassId,
                        include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot).Include(x => x.Course)!);

                    var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                        predicate: c => c.Id == cls.CourseId,
                        include: x => x.Include(x => x.Syllabus)!);

                    itemResponse = CartItemCustomMapper.fromCartItemToCartItemResponse(course, cls, item.Id);
                    itemResponse.Schedules!.Add(ScheduleCustomMapper.fromClassInforToOpeningScheduleResponse(cls));
                    itemResponse.Price = await GetDynamicPrice(cls.Id, true);

                    response.Items.Add(itemResponse);
                    continue;
                }

                if (item.CourseId != default)
                {
                    var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                      predicate: c => c.Id == item.CourseId,
                      include: x => x.Include(x => x.Syllabus)!);

                    var classes = await _unitOfWork.GetRepository<Class>().GetListAsync(
                        predicate: c => c.CourseId == item.CourseId,
                        include: x => x.Include(x => x.Schedules.OrderBy(sc => sc.Date)).ThenInclude(sc => sc.Slot)!);

                    itemResponse = CartItemCustomMapper.fromCartItemToCartItemResponse(course, null, item.Id);
                    itemResponse.Schedules = classes.Select(cls => ScheduleCustomMapper.fromClassInforToOpeningScheduleResponse(cls)).ToList();
                    itemResponse.Price = await GetDynamicPrice(course.Id, false);

                    response.Items.Add(itemResponse);
                }
            }
        }
    }
}
