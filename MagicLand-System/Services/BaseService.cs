using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain.Models.TempEntity.Class;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Security.Claims;

namespace MagicLand_System.Services
{
    public abstract class BaseService<T> where T : class
    {
        protected IUnitOfWork<MagicLandContext> _unitOfWork;
        protected ILogger<T> _logger;
        protected IMapper _mapper;
        protected IHttpContextAccessor _httpContextAccessor;
        public BaseService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<T> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
        protected string GetCurrentUserIpAdress()
        {
            string ipAdrr = _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return ipAdrr;
        }
        protected string GetPhoneFromJwt()
        {
            string phone = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return phone;
        }
        protected string GetRoleFromJwt()
        {
            string role = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
            return role;
        }
        protected Guid GetUserIdFromJwt()
        {
            return Guid.Parse(_httpContextAccessor?.HttpContext?.User?.FindFirstValue("userId"));
        }
        protected async Task<User> GetUserFromJwt()
        {
            Guid id = Guid.Parse(_httpContextAccessor?.HttpContext?.User?.FindFirstValue("userId"));
            //string role = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

            //if (role == "Student")
            //{
            //    var student = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => x.Id == id);
            //    return (default, student);
            //}
            var account = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x.Include(x => x.Role));
            return account;
            //return (account, default);
        }
        protected async Task<double> GetClassPrice(Guid classId)
        {
            double result = 0;

            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == classId);

            var coursePrices = (await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                selector: x => x.CoursePrices,
                predicate: x => x.Id == cls.CourseId))!.ToArray();

            //coursePrices = coursePrices.OrderBy(x => x.EffectiveDate).ToArray();

            result = coursePrices.First().Price;
            for (int i = 1; i < coursePrices.Length; i++)
            {
                //if (cls.AddedDate >= coursePrices[i].EffectiveDate)
                //{
                //    result = coursePrices[i].Price;
                //}
                //else
                //{
                //    break;
                //}
            }

            return result;
        }

        protected async Task<double> GetPriceInTemp(Guid id, bool isClass)
        {
            if (isClass)
            {
                return await _unitOfWork.GetRepository<TempItemPrice>().SingleOrDefaultAsync(predicate: x => x.ClassId == id, selector: x => x.Price);
            }
            return await _unitOfWork.GetRepository<TempItemPrice>().SingleOrDefaultAsync(predicate: x => x.CourseId == id, selector: x => x.Price);
        }
    }

}
