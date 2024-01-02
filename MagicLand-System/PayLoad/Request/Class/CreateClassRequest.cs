using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Class
{
    public class CreateClassRequest
    {
        public Guid CourseId { get; set; }
        public Guid RoomId { get; set; }
        public Guid LecturerId { get; set; }
        public DateTime StartDate { get; set; } 
        public string Method {  get; set; }
        [Required(ErrorMessage = "Limit number student is missing")]
        [Range(1, 100)]
        public int LimitNumberStudent {  get; set; }
        [Required(ErrorMessage = "Class code is missing")]
        public string ClassCode {  get; set; }
        [Required(ErrorMessage = "Min number student is missing")]
        [Range(1, 100)]
        public int LeastNumberStudent {  get; set; } 
        public List<ScheduleRequest> ScheduleRequests { get; set; } = new List<ScheduleRequest>();

    }
}
