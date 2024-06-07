using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class SubDescriptionTitle
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }

        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }

        public ICollection<SubDescriptionContent> SubDescriptionContents { get; set; } = new List<SubDescriptionContent>();
    }
}
