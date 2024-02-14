namespace MagicLand_System.PayLoad.Response.Notifications
{
    public class NotificationResponse
    {
        public Guid NotificationId { get; set; }
        public string? Title { get; set; } = "";
        public string? Body { get; set; } = "";
        public string? Type { get; set; } = "NORMAL";
        public DateTime? CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
        public string? ActionData { get; set; }
    }
}
