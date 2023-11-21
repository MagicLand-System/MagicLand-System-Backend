using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
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
            User account = await _unitOfWork.GetRepository<User>().SingleOrDefaultAsync(predicate: x => x.Id == id, include: x => x.Include(x => x.Role).Include(x => x.Students));
            return account;
        }
    }

}
