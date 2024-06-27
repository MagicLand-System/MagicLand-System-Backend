using MagicLand_System.Enums;
using MagicLand_System.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MagicLand_System.Helpers
{
    public static class DateTimeHelper
    {
        public static string ConvertDateNumberToDayweek(int dateNumber)
        {
            switch (dateNumber)
            {
                case 1:
                    return DateFilter.Sunday.ToString();
                case 2:
                    return "2";
                case 4:
                    return "3";
                case 8:
                    return "4";
                case 16:
                    return "5";
                case 32:
                    return "6";
                case 64:
                    return "7";
                default:
                    return "2";
            }
        }
        public static DateFilter GetDateFromDateTime(DateTime datetime)
        {
            var date = datetime.DayOfWeek;
            switch (date)
            {
                case DayOfWeek.Sunday:
                    return DateFilter.Sunday;
                case DayOfWeek.Monday:
                    return DateFilter.Monday;
                case DayOfWeek.Tuesday:
                    return DateFilter.Tuesday;
                case DayOfWeek.Wednesday:
                    return DateFilter.Wednesday;
                case DayOfWeek.Thursday:
                    return DateFilter.Thursday;
                case DayOfWeek.Friday:
                    return DateFilter.Friday;
                case DayOfWeek.Saturday:
                    return DateFilter.Saturday;
                default:
                    return DateFilter.Monday;
            }
        }
        public static List<DateFilter> GetDatesFromDateFilter(int? dateFilter)
        {
            List<DateFilter> dateFilters = new List<DateFilter>();
            if (dateFilter.HasValue)
            {
                foreach (var date in EnumUtil.GetValues<DateFilter>())
                {
                    if ((dateFilter.Value & (int)date) > 0) dateFilters.Add(date);
                }
            }

            return dateFilters;
        }
        public static TimeOnly ConvertIntToTimeOnly(int timeIntFormat)
        {
            int hour = (int)timeIntFormat / 60;
            decimal minuteInDecimal = (decimal)timeIntFormat / 60;
            decimal minuteHasDecimal = minuteInDecimal - Math.Floor(minuteInDecimal);
            int minute = (int)Math.Ceiling((minuteHasDecimal * 60));
            return new TimeOnly(hour, minute);
        }

    }
}
