using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }

        public string Gender { get; set; }
        public string? AvatarImage { get; set;}
        public DateTime? DateOfBirth { get; set; } = null;
        [ForeignKey("Address")]
        public Guid? AddressId { get; set; } = null;
        public Address Address { get; set; }
        [ForeignKey("Role")]
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        [ForeignKey("Cart")]
        public Guid? CartId { get; set; } = null;
        public Cart Cart { get; set; }
        [ForeignKey("PersonalWallet")]
        public Guid? PersonalWalletId { get; set; } = null;
        public PersonalWallet PersonalWallet { get; set; }
        public ICollection<UserPromotion> UserPromotions { get; set; } = new List<UserPromotion>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<ClassFeeTransaction> ClassFeeTransactions { get; set; } = new List<ClassFeeTransaction>();
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
