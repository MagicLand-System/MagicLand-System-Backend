using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class SessionDescription
    {
        public Guid Id { get; set; }    
        public string Detail { get; set; }  
        public string Content { get; set; }
        public Guid SessionId { get; set; }
        public Session Session { get; set; }    
    }
}
