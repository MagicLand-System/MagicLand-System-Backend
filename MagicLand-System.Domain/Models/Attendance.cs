using System.ComponentModel.DataAnnotations.Schema;

namespace MagicLand_System.Domain.Models
{
    public class Attendance
    {
        public Guid Id { get; set; }
        public bool? IsPresent { get; set; } = default;
        public bool? IsPublic { get; set; } = false;
        public Guid? MakeUpFromScheduleId { get; set; } = null;
        public string? Note { get; set; } = string.Empty;


        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public Student? Student { get; set; }

        [ForeignKey("Schedule")]
        public Guid ScheduleId { get; set; }
        public Schedule? Schedule { get; set; }
    }
}
