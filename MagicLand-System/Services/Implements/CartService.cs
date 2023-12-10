using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.CustomMapper;
using MagicLand_System.PayLoad.Response.Cart;
using MagicLand_System.PayLoad.Response.Class;
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

        public async Task<CartResponse> ModifyCartOffCurrentParentAsync(List<Guid> studentIds, Guid classId)
        {
            var currentParentCart = await FetchCurrentParentCart();

            var cartReponse = new CartResponse();
            try
            {
                if (currentParentCart.CartItems.Count() > 0 && currentParentCart.CartItems.Any(x => x.ClassId == classId))
                {
                    var sameCurrentCartItem = currentParentCart.CartItems.SingleOrDefault(x => x.ClassId == classId);

                    _unitOfWork.GetRepository<StudentInCart>().DeleteRangeAsync(sameCurrentCartItem!.StudentInCarts);

                    await _unitOfWork.GetRepository<StudentInCart>().InsertRangeAsync
                        (
                             RenderStudentInClass(studentIds, sameCurrentCartItem)
                        );

                    cartReponse = await _unitOfWork.CommitAsync() > 0
                        ? await GetCartOfCurrentParentAsync()
                        : throw new BadHttpRequestException("Uncatch Exception In Commit Database", StatusCodes.Status500InternalServerError);
                }
                else
                {
                    var newItem = new CartItem
                    {
                        Id = new Guid(),
                        CartId = currentParentCart.Id,
                        ClassId = classId
                    };

                    await _unitOfWork.GetRepository<CartItem>().InsertAsync(newItem);

                    await _unitOfWork.GetRepository<StudentInCart>().InsertRangeAsync
                     (
                          RenderStudentInClass(studentIds, newItem)
                     );

                    cartReponse = await _unitOfWork.CommitAsync() > 0
                        ? await GetCartOfCurrentParentAsync()
                        : throw new BadHttpRequestException("Uncatch Exception In Commit Database", StatusCodes.Status500InternalServerError);
                }

                return cartReponse;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.InnerException != null ? ex.InnerException.Message : ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<Cart> FetchCurrentParentCart()
        {
            return await _unitOfWork.GetRepository<Cart>().SingleOrDefaultAsync(
                predicate: x => x.UserId == GetUserIdFromJwt(),
                include: x => x.Include(x => x.CartItems).ThenInclude(cts => cts.StudentInCarts));
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

        public async Task<CartResponse> GetCartOfCurrentParentAsync()
        {
            try
            {
                var currentParrentCart = await FetchCurrentParentCart();

                if (currentParrentCart != null && currentParrentCart.CartItems.Count() > 0)
                {
                    var classes = new List<ClassResponse>();
                    foreach (var task in currentParrentCart.CartItems.Select(async cartItem => await _unitOfWork.GetRepository<Class>()
                    .SingleOrDefaultAsync(predicate: x => x.Id == cartItem.ClassId)))
                    {
                        var cls = await task;
                        classes.Add(_mapper.Map<ClassResponse>(cls));
                    }

                    var students = new List<Student>();
                    foreach (var task in currentParrentCart.CartItems.SelectMany(c => c.StudentInCarts)
                        .Where(cartItemRelation => cartItemRelation != null)
                        .Select(async cartItemRelation => await _unitOfWork.GetRepository<Student>()
                        .SingleOrDefaultAsync(predicate: c => c.Id == cartItemRelation.StudentId)))
                    {
                        var student = await task;
                        students.Add(student);
                    }

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
                    return CustomMapper.fromCartToCartResponse(currentParrentCart, students, classes);
                }

                return new CartResponse { Id = currentParrentCart != null ? currentParrentCart.Id : default };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteItemInCartOfCurrentParentAsync(Guid itemId)
        {
            try
            {
                var currentParentCart = await FetchCurrentParentCart();
                var cartItemDelete = currentParentCart.CartItems.SingleOrDefault(x => x.Id == itemId);
                if (cartItemDelete == null)
                {
                    return false;
                }
                _unitOfWork.GetRepository<CartItem>().DeleteAsync(cartItemDelete);

                return await _unitOfWork.CommitAsync() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
