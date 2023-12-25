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

                    if(studentIds.Count() > 0)
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
                        ClassId = classId
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
                    var classes = new List<ClassResponse>();
                    foreach (var task in currentParrentCart.CartItems.Select(async cartItem => await _unitOfWork.GetRepository<Class>()
                    .SingleOrDefaultAsync(predicate: x => x.Id == cartItem.ClassId, include: x => x
                    .Include(x => x.Lecture!)
                    .Include(x => x.StudentClasses)
                    .Include(x => x.Schedules.OrderBy(sc => sc.Date))
                    .ThenInclude(s => s.Slot)!
                    .Include(x => x.Schedules.OrderBy(sc => sc.Date))
                    .ThenInclude(s => s.Room)!)))
                    {
                        var cls = await task;
                        classes.Add(_mapper.Map<ClassResponse>(cls));
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
        public async Task<BillResponse> CheckOutCartAsync(List<CartItemResponse> cartItems)
        {
            double actualTotal = 0;
            var newTransactions = new List<WalletTransaction>();
            var newStudentInClass = new List<StudentClass>();

            var currentPayer = await GetUserFromJwt();

            var personalWallet = await _unitOfWork.GetRepository<PersonalWallet>()
                   .SingleOrDefaultAsync(predicate: x => x.UserId.Equals(GetUserIdFromJwt()));

            foreach (var item in cartItems)
            {
                var price = await _unitOfWork.GetRepository<Course>()
                .SingleOrDefaultAsync(selector: x => x.Price, predicate: x => x.Classes.Any(c => c.Id.Equals(item.Class.Id)));

                double total = CalculateTotal(item.Students.Count(), price);
                actualTotal += total;

                var newTransaction = new WalletTransaction
                {
                    Id = new Guid(),
                    Money = total,
                    Type = CheckOutMethodEnum.SystemWallet.ToString(),
                    Description = $"Registered class {item.Class.Name} via cart",
                    CreatedTime = DateTime.Now,
                };

                var studentInClasses = item.Students.Select(stu =>
                new StudentClass
                {
                    Id = new Guid(),
                    Status = "NORMAL",
                    StudentId = stu.Id,
                    ClassId = item.Id,
                }).ToList();


                newTransactions.Add(newTransaction);
                newStudentInClass.AddRange(studentInClasses);
            }

            try
            {
                if (personalWallet.Balance < actualTotal)
                {
                    throw new BadHttpRequestException("Your balance not enough for register all class selected", StatusCodes.Status400BadRequest);
                }

                personalWallet.Balance = personalWallet.Balance - actualTotal;

                _unitOfWork.GetRepository<PersonalWallet>().UpdateAsync(personalWallet);
                await _unitOfWork.GetRepository<WalletTransaction>().InsertRangeAsync(newTransactions);
                await _unitOfWork.GetRepository<StudentClass>().InsertRangeAsync(newStudentInClass);

                await _unitOfWork.CommitAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException!.ToString());
            }

            var bill = new BillResponse
            {
                Status = "Purchase Success",
                Date = DateTime.Now,
                Method = CheckOutMethodEnum.SystemWallet,
                Payer = currentPayer.FullName!,
                MoneyPaid = actualTotal,
            };

            return bill;
        }
        protected double CalculateTotal(int numberStudents, double price)
        {
            return price * numberStudents;
            //Apply promotion/sales
        }
    }
}
