using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; } = null!;
        public string Gender { get; set; } 
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }   
        public Guid RoleId { get; set; } 
        public Role Role { get; set; }
    }
}
