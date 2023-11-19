using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Room
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? Floor { get; set; }
        public string? Status { get; set; }
        public string? LinkURL { get; set; } 
        public ICollection<Session> Sessions  { get; set; } = new List<Session>();
    }
}
