using MagicLand_System.Helpers;

namespace MagicLand_System.Background
{
    public class BackgoundTime
    {
        public static DateTime GetTime()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            string days = configuration.GetSection("DateNumber:Days").Value!;
            string hours = configuration.GetSection("DateNumber:Hours").Value!;
            string minutes = configuration.GetSection("DateNumber:Minutes").Value!;

            DateTime dateTime = DateTime.Today
            .AddDays(int.Parse(days))
            .AddHours(int.Parse(hours))
            .AddMinutes(int.Parse(minutes));

            return dateTime;
        }

    }
}
