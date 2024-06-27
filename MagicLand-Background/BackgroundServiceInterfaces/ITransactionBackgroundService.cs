namespace MagicLand_System.Background.BackgroundServiceInterfaces
{
    public interface ITransactionBackgroundService
    {
        internal Task<string> UpdateTransactionAfterTime();
    }
}
