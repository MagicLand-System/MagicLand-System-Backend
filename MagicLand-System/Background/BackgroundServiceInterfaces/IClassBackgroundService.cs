namespace MagicLand_System.Background.BackgroundServiceInterfaces
{
    public interface IClassBackgroundService
    {
        internal Task<string> UpdateClassInTimeAsync();
        internal Task<string> UpdateAttendanceInTimeAsync();
    }
}
