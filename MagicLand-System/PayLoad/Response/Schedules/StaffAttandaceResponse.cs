using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Classes;

namespace MagicLand_System.PayLoad.Response.Schedules
{
    public class StaffAttandaceResponse
    {
        public Guid Id { get; set; }
        public Student Student { get; set; }
        public DateTime? Day { get; set; }
        public string IsPresent { get; set; }
        public ClassResponse Class { get; set; }
    }
}
