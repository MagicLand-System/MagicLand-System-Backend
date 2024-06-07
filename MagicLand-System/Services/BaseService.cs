using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
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

            var account = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x.Include(x => x.Role).Include(x => x.PersonalWallet));
            return account;
        }

        protected DateTime GetCurrentTime()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            string days = configuration.GetSection("DateNumber:Days").Value!;
            string hours = configuration.GetSection("DateNumber:Hours").Value!;
            string minutes = configuration.GetSection("DateNumber:Minutes").Value!;


            if (hours != "0" || days != "0" || minutes != "0")
            {
                return DateTime.Today
                      .AddDays(int.Parse(days))
                      .AddHours(int.Parse(hours))
                      .AddMinutes(int.Parse(minutes));
            }
            else
            {
                return DateTime.Now;
            }
        }

        protected async Task<double> GetDynamicPrice(Guid id, bool isClass)
        {
            var coursePrices = isClass
              ? await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                predicate: x => x.Classes.Any(cls => cls.Id == id),
                selector: x => x.CoursePrices)
              : await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                predicate: x => x.Id == id,
                selector: x => x.CoursePrices);

            if (coursePrices == null || coursePrices.Count == 0)
            {
                return 0;
            }

            var prices = coursePrices.Where(x => x.EndDate < DateTime.Now.AddYears(10)).ToList();

            foreach (var pr in prices)
            {
                if (pr.StartDate <= DateTime.Now && pr.EndDate >= DateTime.Now)
                {
                    return pr.Price;
                }
            }

            return (coursePrices.OrderByDescending(x => x.EndDate).ToArray())[0].Price;
        }

        protected bool IsAuthorized()
        {
            var httpContext = _httpContextAccessor.HttpContext;


            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }
            return true;
        }
    }

}
