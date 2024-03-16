using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class CoursePrice
    {
        public Guid Id { get; set; }
        public double Price { get; set; }
        public DateTime EffectiveDate { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; }
    }
}
