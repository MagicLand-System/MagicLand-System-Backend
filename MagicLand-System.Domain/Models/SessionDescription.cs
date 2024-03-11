using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class SessionDescription
    {
        public Guid Id { get; set; }    
        public string? Detail { get; set; }  
        public string? Content { get; set; }
        public int Order { get; set; }

        [ForeignKey("Session")]
        public Guid? SessionId { get; set; }
        public Session? Session { get; set; }    
    }
}
