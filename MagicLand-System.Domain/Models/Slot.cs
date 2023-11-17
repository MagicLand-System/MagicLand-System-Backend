using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Slot
    {
        public Guid Id { get; set; }
        public string StartTime { get; set; }   
        public string EndTime { get; set; }
        public ICollection<Session> Sessions { get; set;} = new List<Session>();
    }
}
