using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string? Title { get; set; } = "";
        public string? Body { get; set; } = "";
        public string? Priority { get; set; } = "NORMAL";
        public string? Image { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? NotificationTimer { get; set; }
        public bool IsRead { get; set; } = false;
        public string? ActionData { get; set; }


        [ForeignKey("User")]
        public Guid? UserId { get; set; }
        public User? TargetUser { get; set; }
    }
}
