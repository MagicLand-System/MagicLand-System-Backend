using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class SubDescriptionContent
    {
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }


        [ForeignKey("SubDescriptionTitle")]
        public Guid SubDescriptionTitleId { get; set; }
        public SubDescriptionTitle SubDescriptionTitle { get; set; } = new SubDescriptionTitle();
    }
}
