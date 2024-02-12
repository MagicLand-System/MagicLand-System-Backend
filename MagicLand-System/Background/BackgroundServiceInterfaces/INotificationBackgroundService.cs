namespace MagicLand_System.Background.BackgroundServiceInterfaces
{
    public interface INotificationBackgroundService
    {
        internal Task<string> CreateNotificationInCondition();
    }
}
