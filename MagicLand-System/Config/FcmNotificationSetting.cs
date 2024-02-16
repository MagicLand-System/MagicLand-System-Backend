namespace MagicLand_System.Config
{
    public class FcmNotificationSetting
    {
        public static string ConfigName => "FcmNotification";
        public string SenderId { get; set; }
        public string ServerKey { get; set; }
    }
}
