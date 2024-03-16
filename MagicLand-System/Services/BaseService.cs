﻿using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
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
            var classx = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate : x => x.Id == classId);
            var course = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(predicate : x => x.Id.ToString().Equals(classx.CourseId.ToString()),include : x => x.Include(x => x.CoursePrices));
            var prices = course.CoursePrices;
            var classDate = classx.AddedDate;
            var priceList = prices.OrderBy(x => x.EffectiveDate).ToArray();
            if(classDate < priceList[0].EffectiveDate)
            {
                return result;
            }
            for ( var i = 0; i < priceList.Length - 1; i++)
            {
                if (priceList[i].EffectiveDate <= classDate && classDate < priceList[i+1].EffectiveDate)
                {
                    return result = priceList[i].Price;
                }
            }
            return priceList[priceList.Length - 1].Price;
        }
    }

}
