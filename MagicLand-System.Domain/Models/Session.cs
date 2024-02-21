using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public int NoSession { get; set; }
        public string? Content { get; set; }
        public string? Description { get; set; }

        [ForeignKey("Topic")]
        public Guid TopicId { get; set; }
        public Topic? Topic { get; set; }
    }
}
