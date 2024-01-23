using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Class
{
    public class UpdateClassRequest
    {
        public Guid? CourseId { get; set; }
        public Guid? RoomId { get; set; }
        public Guid? LecturerId { get; set; }
        public string Method { get; set; }
        [Required(ErrorMessage = "Limit number student is missing")]
        [Range(1, 100)]
        public int? LimitNumberStudent { get; set; }
        [Required(ErrorMessage = "Minimum number student is missing")]
        [Range(1, 100)]
        public int? LeastNumberStudent { get; set; }
    }
}
