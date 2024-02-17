using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Class
{
    public class CreateClassesRequest
    {
        public int Index { get; set; }
        public string CourseName { get; set; }
        public string RoomName { get; set; }
        public string LecturerPhone { get; set; }
        public DateTime StartDate { get; set; }
        public string Method { get; set; }
        [Required(ErrorMessage = "Limit number student is missing")]
        [Range(1, 100)]
        public int LimitNumberStudent { get; set; }
        [Required(ErrorMessage = "Class code is missing")]
        public string ClassCode { get; set; }
        [Required(ErrorMessage = "Min number student is missing")]
        [Range(1, 100)]
        public int LeastNumberStudent { get; set; }
        public List<ScheduleRequestV2> ScheduleRequests { get; set; } = new List<ScheduleRequestV2>();
    }
}
