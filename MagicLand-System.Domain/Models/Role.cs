using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<User> Accounts { get; set; } = new List<User>();

    }
}
