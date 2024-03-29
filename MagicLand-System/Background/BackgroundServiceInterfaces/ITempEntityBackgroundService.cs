namespace MagicLand_System.Background.BackgroundServiceInterfaces
{
    public interface ITempEntityBackgroundService
    {
        internal Task<string> DeleteTempEntityByCondition();
    }
}
