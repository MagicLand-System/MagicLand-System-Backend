using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    //Delete If Need
    public class StudentTransaction
    {
        public Guid Id { get; set; }
        //[ForeignKey("Student")]
        //public Guid StudentId { get; set; }
        //public Student Student { get; set; }
        //[ForeignKey("ClassTransaction")]
        public Guid ClassTransactionId { get; set; }
        public ClassTransaction ClassTransaction { get; set; }

    }
}
