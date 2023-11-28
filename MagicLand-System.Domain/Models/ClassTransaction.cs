using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    //Delete If Need
    public class ClassTransaction
    {
        public Guid Id { get; set; }
        [ForeignKey("Class")]
        //public Guid ClassId { get; set; }
        //public Class Class { get; set; }    
        //[ForeignKey("ClassFeeTransaction")]
        public Guid ClassFeeTransactionId { get; set; }
        public ClassFeeTransaction ClassFeeTransaction { get; set; }
        public ICollection<StudentTransaction> StudentTransactions { get; set; } = new List<StudentTransaction>();
    }
}
