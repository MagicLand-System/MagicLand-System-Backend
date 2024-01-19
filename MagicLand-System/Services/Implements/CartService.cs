using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.CustomMapper;
using MagicLand_System.PayLoad.Response.Carts;
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
        public async Task<FavoriteResponse> AddCourseFavoriteOffCurrentParentAsync(Guid courseId)
        {
            var currentParentCart = await FetchCurrentParentCart();

            var favoriteResponse = new FavoriteResponse();
            try
            {
                if (currentParentCart.CartItems.Count() > 0 && currentParentCart.CartItems.Any(x => x.CourseId == courseId))
                {
                    throw new BadHttpRequestException($"Id [{courseId}] Của Khóa Đã Có Trong Danh Sách Yêu Thích", StatusCodes.Status400BadRequest);
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

                    favoriteResponse = await _unitOfWork.CommitAsync() > 0
                         ? await GetDetailCurrentParrentFavorite()
                         : throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh", StatusCodes.Status500InternalServerError);
                }

                return favoriteResponse;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.InnerException != null ? ex.InnerException.Message : ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<CartResponse> ModifyCartOffCurrentParentAsync(List<Guid> studentIds, Guid classId)
        {
            var currentParentCart = await FetchCurrentParentCart();

            var cartReponse = new CartResponse();
            try
            {
                if (currentParentCart.CartItems.Count() > 0 && currentParentCart.CartItems.Any(x => x.ClassId == classId))
                {
                    var currentCartItem = currentParentCart.CartItems.SingleOrDefault(x => x.ClassId == classId);

                    if (studentIds.Count() == 0 && currentCartItem!.StudentInCarts.Count() == 0)
                    {
                        throw new BadHttpRequestException($"Id [{classId}] Của Lớp Đã Có Trong Giỏ Hàng", StatusCodes.Status400BadRequest);
                    }

                    if (currentCartItem!.StudentInCarts.Select(sic => sic.StudentId).ToList().SequenceEqual(studentIds))
                    {
                        throw new BadHttpRequestException($"Bạn Đã Có Id [{string.Join(", ", studentIds)}] Trong Lớp [{classId}] Ở Giỏ Hàng", StatusCodes.Status400BadRequest);
                    }

                    _unitOfWork.GetRepository<StudentInCart>().DeleteRangeAsync(currentCartItem!.StudentInCarts);

                    if (studentIds.Count() > 0)
                    {
                        await _unitOfWork.GetRepository<StudentInCart>().InsertRangeAsync
                       (
                            RenderStudentInClass(studentIds, currentCartItem)
                       );
                    }

                    cartReponse = await _unitOfWork.CommitAsync() > 0
                        ? await GetDetailCurrentParrentCart()
                        : throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh", StatusCodes.Status500InternalServerError);
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

                    cartReponse = await _unitOfWork.CommitAsync() > 0
                         ? await GetDetailCurrentParrentCart()
                         : throw new BadHttpRequestException("Lỗi Hệ Thống Phát Sinh", StatusCodes.Status500InternalServerError);
                }

                return cartReponse;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.InnerException != null ? ex.InnerException.Message : ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<CartResponse> GetDetailCurrentParrentCart()
        {
            try
            {
                var currentParrentCart = await FetchCurrentParentCart();

                if (currentParrentCart != null && currentParrentCart.CartItems.Count() > 0)
                {
                    var validItems = currentParrentCart.CartItems.Where(ci => ci.CourseId == default).ToList();

                    var classes = new List<ClassResExtraInfor>();

                    foreach (var task in validItems.Select(async cartItem => await _unitOfWork.GetRepository<Class>()
                    .SingleOrDefaultAsync(predicate: x => x.Id == cartItem.ClassId, include: x => x
                    .Include(x => x.Lecture!)
                    .Include(x => x.StudentClasses)
                    .Include(x => x.Course)
                    .Include(x => x.Schedules.OrderBy(sc => sc.Date))
                    .ThenInclude(s => s.Slot)!
                    .Include(x => x.Schedules.OrderBy(sc => sc.Date))
                    .ThenInclude(s => s.Room)!)))
                    {
                        var cls = await task;
                        classes.Add(_mapper.Map<ClassResExtraInfor>(cls));
                    }

                    var students = new List<Student>();

                    foreach (var task in currentParrentCart.CartItems.SelectMany(c => c.StudentInCarts)
                        .Where(studentInCart => studentInCart != null)
                        .Select(async studentInCart => await _unitOfWork.GetRepository<Student>()
                        .SingleOrDefaultAsync(predicate: c => c.Id == studentInCart.StudentId)))
                    {
                        var student = await task;
                        students.Add(student);
                    }
                    #region
                    // Leave InCase Using Back

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
                    return CartCustomMapper.fromCartToCartResponse(currentParrentCart, students, classes);
                }

                return new CartResponse { CartId = currentParrentCart != null ? currentParrentCart.Id : default };

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

                        courses.Add(course);

                    }

                    return CartCustomMapper.fromCartToFavoriteResponse(currentParrentCart.Id, itemFavorites, courses);
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

    }
}
