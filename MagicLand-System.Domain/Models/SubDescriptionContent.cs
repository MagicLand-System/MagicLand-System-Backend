using System.ComponentModel.DataAnnotations.Schema;

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
