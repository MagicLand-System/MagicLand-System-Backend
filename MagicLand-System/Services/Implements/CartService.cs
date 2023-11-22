using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.CustomMapper;
using MagicLand_System.PayLoad.Response.Cart;
using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.Repository.Implement;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Finance.Implementations;
using System.Runtime.ConstrainedExecution;

namespace MagicLand_System.Services.Implements
{
    public class CartService : BaseService<CartService>, ICartService
    {
        public CartService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<CartService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<string> AddCartAsync(List<Guid> studentIds, Guid classId)
        {
            var currentParentCart = await _unitOfWork.GetRepository<Cart>().SingleOrDefaultAsync(
                predicate: x => x.UserId == GetUserIdFromJwt(),
                include: x => x.Include(x => x.Carts).ThenInclude(cts => cts.CartItemRelations));

            string result = "Add Success";
            try
            {
                if (currentParentCart.Carts.Count() > 0 && currentParentCart.Carts.Any(x => x.ClassId == classId))
                {
                    var sameCurrentCartItem = currentParentCart.Carts.SingleOrDefault(x => x.ClassId == classId);

                    _unitOfWork.GetRepository<CartItemRelation>().DeleteRangeAsync(sameCurrentCartItem!.CartItemRelations);                

                    await _unitOfWork.GetRepository<CartItemRelation>().InsertRangeAsync
                        (
                             RenderStudentInClass(studentIds, sameCurrentCartItem)
                        );

                    result = await _unitOfWork.CommitAsync() > 0 ? "Update Success" : "Uncatch Error Exception!";
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

                    await _unitOfWork.GetRepository<CartItemRelation>().InsertRangeAsync
                     (
                          RenderStudentInClass(studentIds, newItem)
                     );

                    result = await _unitOfWork.CommitAsync() > 1 ? result : "Add Failed";
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.InnerException != null ? ex.InnerException.Message : ex.Message, StatusCodes.Status500InternalServerError);
            }
        }

        private List<CartItemRelation> RenderStudentInClass(List<Guid> studentIds, CartItem cartItem)
        {
            return studentIds.Select(s => new CartItemRelation
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
                var cart = await _unitOfWork.GetRepository<Cart>().SingleOrDefaultAsync(predicate: x => x.UserId == GetUserIdFromJwt(),
                include: x => x.Include(x => x.Carts).ThenInclude(c => c.CartItemRelations));

                if (cart != null && cart.Carts.Count() > 0)
                {
                    var classes = new List<ClassResponse>();
                    foreach(var task in cart.Carts.Select(async cartItem => await _unitOfWork.GetRepository<Class>()
                    .SingleOrDefaultAsync( predicate: x => x.Id == cartItem.ClassId, include: x => x.Include(x => x.User).Include(x => x.Address)!)))
                    {
                        var cls = await task;
                        classes.Add(_mapper.Map<ClassResponse>(cls));
                    }

                    var students = new List<Student>();
                    foreach (var task in cart.Carts.SelectMany(c => c.CartItemRelations)
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
                    return CustomMapper.fromCartToCartResponse(cart, students, classes);
                }

                return new CartResponse { Id = cart != null ? cart.Id : default };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
