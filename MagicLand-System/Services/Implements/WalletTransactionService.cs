using AutoMapper;
using MagicLand_System.Domain;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.PayLoad.Response.WalletTransactions;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MagicLand_System.Services.Implements
{
    public class WalletTransactionService : BaseService<WalletTransactionService>, IWalletTransactionService
    {
        public WalletTransactionService(IUnitOfWork<MagicLandContext> unitOfWork, ILogger<WalletTransactionService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
        }

        public async Task<WalletTransactionResponse> GetWalletTransaction(string id)
        {
            var transactions = await GetWalletTransactions();
            if (transactions == null || transactions.Count == 0)
            {
                return new WalletTransactionResponse();
            }
            return transactions.SingleOrDefault(x => x.TransactionId.ToString().ToLower().Equals(id.ToLower()));
        }

        public async Task<List<WalletTransactionResponse>> GetWalletTransactions(string phone = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = await _unitOfWork.GetRepository<WalletTransaction>().GetListAsync(include : x => x.Include(x => x.PersonalWallet).ThenInclude(x => x.User).ThenInclude(x => x.Students));
            if(transactions == null || transactions.Count == 0) 
            {
                return new List<WalletTransactionResponse>();
            }
            List<WalletTransactionResponse> result = new List<WalletTransactionResponse>();
            foreach(var transaction in transactions)
            {
                var description = transaction.Description;
                var parts = description.Split('[', ']', ':', ',');
                var cleanParts = parts.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                var classCode = cleanParts[1];
                var studentName = cleanParts[5];
                List<string> names = new List<string>();    
                for(int i = 5;i <= cleanParts.Length - 1; i++)
                {
                    names.Add(cleanParts[i]);
                }
                User user = transaction.PersonalWallet.User;
                Class myclass = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate : x => x.ClassCode.ToLower().Equals(classCode.ToLower()));
                List<Student> students = new List<Student>();
                foreach (var namex in names) 
                {
                    var studentMatch = await _unitOfWork.GetRepository<Student>().SingleOrDefaultAsync(predicate: x => (x.ParentId.ToString().Equals(user.Id.ToString()) && x.FullName.Trim().ToLower().Equals(namex.Trim().ToLower())));
                    students.Add(studentMatch);
                }
                WalletTransactionResponse response = new WalletTransactionResponse
                {
                    Description = description,
                    CreatedTime = transaction.CreatedTime,
                    Money = transaction.Money,
                    Parent = new PayLoad.Response.Users.UserResponse
                    {
                        AvatarImage = user.AvatarImage,
                        DateOfBirth = user.DateOfBirth,
                        Email = user.Email,
                        FullName = user.FullName,
                        Gender = user.Gender,
                        Id = transaction.Id,
                        Phone = user.Phone,
                    },
                    Type = "SystemWallet",
                    TransactionId = transaction.Id,
                    MyClassResponse = myclass,
                    Students = students
                };
                result.Add(response);
            }
            if(endDate != null) { endDate = endDate.Value.AddHours(23).AddMinutes(59); }
            result = (result.OrderByDescending(x => x.CreatedTime)).ToList();
            if (phone == null && startDate == null && endDate == null)
            {
                return (result.OrderByDescending(x => x.CreatedTime)).ToList();
            }
            if(phone != null && startDate == null && endDate == null) 
            {
                return (result.Where(x => x.Parent.Phone.ToLower().Equals(phone.ToLower()))).ToList();
            }
            if(phone == null && startDate != null && endDate == null) 
            {
               return result = (result.Where(x => x.CreatedTime >= startDate)).ToList();
            }
            if (phone != null && startDate != null && endDate == null)
            {
                return (result.Where(x => (x.CreatedTime >= startDate && x.Parent.Phone.ToLower().Equals(phone.ToLower())))).ToList();
            }
            if (phone == null && startDate != null && endDate != null)
            {
                return (result.Where(x => (x.CreatedTime >= startDate && x.CreatedTime <= endDate))).ToList();
            }
            if (phone == null && startDate == null && endDate != null)
            {
                return (result.Where(x => ( x.CreatedTime <= endDate))).ToList();
            }
            if (phone != null && startDate == null && endDate != null)
            {
                return (result.Where(x => (x.CreatedTime <= endDate && x.Parent.Phone.Equals(phone)))).ToList();
            }
            if (endDate < startDate)
            {
                return new List<WalletTransactionResponse>();
            }
            return (result.Where(x => (x.Parent.Phone.ToLower().Equals(phone.ToLower()) && x.CreatedTime >= startDate && x.CreatedTime <= endDate))).ToList();
        }
    }
}
