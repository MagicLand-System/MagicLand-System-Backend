using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class SideFlashCard
    {
        public Guid Id { get; set; }
        public string? Description {  get; set; }
        public string? Image { get; set; }
        public string? Side { get; set; }


        [ForeignKey("FlashCard")]
        public Guid FlashCardId { get; set; }
        public FlashCard? FlashCard { get; set; }
    }
}
