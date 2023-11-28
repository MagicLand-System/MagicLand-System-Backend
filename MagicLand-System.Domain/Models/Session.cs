using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public int NoSession { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }

        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public required Course Course { get; set; }
    }
}
