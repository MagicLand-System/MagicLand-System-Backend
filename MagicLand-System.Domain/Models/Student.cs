using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Student
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? AvatarImage { get; set; }

        [ForeignKey("User")]
        public Guid ParentId { get;set; }
        public required User User { get; set; }
       
       
        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    }
}
