using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Enums;
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
                    var currentCartItem = currentParentCart.CartItems.SingleOrDefault(x => x.ClassId == classId);

                    if (studentIds.Count() == 0 && currentCartItem!.StudentInCarts.Count() == 0)
                    {
                        throw new BadHttpRequestException("You are already add cart this class", StatusCodes.Status400BadRequest);
                    }

                    if (currentCartItem!.StudentInCarts.Select(sic => sic.StudentId).ToList().SequenceEqual(studentIds))
                    {
                        throw new BadHttpRequestException("You are already add registered all student request to this class", StatusCodes.Status400BadRequest);
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
                        : throw new BadHttpRequestException("Uncatch Exception In Commit Database", StatusCodes.Status500InternalServerError);
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
                         : throw new BadHttpRequestException("Uncatch Exception In Commit Database", StatusCodes.Status500InternalServerError);
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
                    var classes = new List<ClassResponseV1>();

                    foreach (var task in currentParrentCart.CartItems.Select(async cartItem => await _unitOfWork.GetRepository<Class>()
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
                        classes.Add(_mapper.Map<ClassResponseV1>(cls));
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
        public async Task<BillResponse> CheckOutCartAsync(List<CartItemResponse> cartItems)
        {
            double total = 0;

            var newStudentInClass = new List<StudentClass>();

            var classes = await ValidateScheduleOfEachItem(cartItems);

            foreach (var item in cartItems)
            {
                total += classes.Single(x => x.Id == item.Class.Id).Course!.Price * item.Students.Count();

                var studentInClasses = item.Students.Select(stu =>
                new StudentClass
                {
                    Id = new Guid(),
                    StudentId = stu.Id,
                    ClassId = item.Class.Id,
                }).ToList();

                newStudentInClass.AddRange(studentInClasses);
            }

            await PurchaseProgress(cartItems, total, newStudentInClass);

            var currentPayer = await GetUserFromJwt();

            var bill = new BillResponse
            {
                Status = "Purchase Success",
                Message = "All students has been registered to all class request",
                Cost = total,
                Discount = CaculateDiscount(total, cartItems.Count()),
                MoneyPaid = total - CaculateDiscount(total, cartItems.Count()),
                Date = DateTime.Now,
                Method = CheckOutMethodEnum.SystemWallet.ToString(),
                Payer = currentPayer.FullName!,
            };

            return bill;
        }

        private async Task<List<Class>> ValidateScheduleOfEachItem(List<CartItemResponse> cartItems)
        {
            var classes = new List<Class>();

            foreach (var item in cartItems)
            {
                var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == item.Class.Id, include: x => x
                .Include(x => x.Course)
                .Include(x => x.Schedules)
                .ThenInclude(s => s.Slot)!);

                classes.Add(cls);
            }

            for (int i = 0; i < classes.Count - 1; i++)
            {
                var coincideSchedule = classes[i].Schedules.Where(sa => classes[i + 1].Schedules
                .Any(sb => sa.Date == sb.Date && sa.Slot!.StartTime == sb.Slot!.StartTime)).FirstOrDefault();

                if (coincideSchedule != null)
                {
                    throw new BadHttpRequestException($"We found that the class {classes[i].Name} is coincide slot start time " +
                        $"{coincideSchedule.Slot!.StartTime} with class {classes[i + 1].Name} \n" +
                        "it may cause an issue to your childs please chose one of two class you're prefer to assign to your childs",
                      StatusCodes.Status400BadRequest);
                }
            }

            return classes;
        }

        private async Task PurchaseProgress(List<CartItemResponse> cartItems, double total, List<StudentClass> newStudentInClass)
        {
            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>()
                  .SingleOrDefaultAsync(predicate: x => x.UserId.Equals(GetUserIdFromJwt()));

            double actualCost = total - CaculateDiscount(total, cartItems.Count());

            if (personalWallet.Balance < actualCost)
            {
                throw new BadHttpRequestException("Your balance not enough for purchase all item selected balance require greater: " + actualCost + "đ",
                    StatusCodes.Status400BadRequest);
            }

            try
            {
                var currentParrentCart = await FetchCurrentParentCart();

                var newTransaction = new WalletTransaction
                {
                    Id = new Guid(),
                    Money = actualCost,
                    Type = CheckOutMethodEnum.SystemWallet.ToString(),
                    Description = "Purchase items selected in cart: " + string.Join(" And ", cartItems.Select(x => x.Id).ToArray()),
                    CreatedTime = DateTime.Now,
                    PersonalWalletId = personalWallet.Id,
                    PersonalWallet = personalWallet,
                };

                personalWallet.Balance -= actualCost;

                var itemDeleted = cartItems.Select(c => currentParrentCart.CartItems.SingleOrDefault(x => x.Id == c.Id)).ToList();

                await _unitOfWork.GetRepository<WalletTransaction>().InsertAsync(newTransaction);
                await _unitOfWork.GetRepository<StudentClass>().InsertRangeAsync(newStudentInClass);
                _unitOfWork.GetRepository<PersonalWallet>().UpdateAsync(personalWallet);
                _unitOfWork.GetRepository<CartItem>().DeleteRangeAsync(itemDeleted!);

                await _unitOfWork.CommitAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException!.ToString());
            }
        }

        private double CaculateDiscount(double total, int numberItem)
        {
            double discount = numberItem >= 2
                   ? (total * 10) / 100
                   : 0.0;
            
            return discount;
        }


    }
}
