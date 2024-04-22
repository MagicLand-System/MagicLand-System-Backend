namespace MagicLand_System.Background.BackgroundServiceInterfaces
{
    public interface INotificationBackgroundService
    {
        //internal Task<string> ModifyNotificationAfterTime();
        internal Task<string> CreateNewNotificationInCondition();
        internal Task<string> CreateNotificationForLastRegisterTime();
        internal Task<string> CreateNotificationForRemindRegisterCourse();
        internal Task<string> PushNotificationRealTime();
    }
}
