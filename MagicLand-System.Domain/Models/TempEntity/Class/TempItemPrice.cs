using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models.TempEntity.Class
{
    public class TempItemPrice
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        public Guid CourseId { get; set; }
        public double Price { get; set; }

    }
}
