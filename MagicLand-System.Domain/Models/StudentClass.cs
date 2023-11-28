using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class StudentClass
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }

        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public Student? Student { get; set; }

        [ForeignKey("Class")]
        public Guid ClassId { get; set; }
        public Class? Class { get; set; }
    }
}
