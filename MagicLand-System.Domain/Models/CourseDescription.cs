using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class CourseDescription
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int Order { get; set; }


        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }

    }
}
