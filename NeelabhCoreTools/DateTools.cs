using System;
using System.Globalization;

public static class DateTools
{
    public static DateTime CurrentDateTime()
    {
        return DateTime.Now;
    }

    public static string GetDate(string format = "dd-MMM-yyyy")
    {
        return DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string GetDateTime(string format = "dd-MMM-yyyy hh:mm:ss tt")
    {
        return DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string FormatDate(this DateTime date, string format = "dd-MMM-yyyy")
    {
        return date.ToString(format, CultureInfo.InvariantCulture);
    }

    public static DateTime FirstDateOfMonth(this DateTime date)
    {
       return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime LastDateOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }

    public static bool HasExpired(this DateTime date)
    {
        return date > CurrentDateTime();
    }

    public static string RelativeTime(this DateTime date)
    {
        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;

        var ts = new TimeSpan(DateTime.Now.Ticks - date.Ticks);
        double delta = Math.Abs(ts.TotalSeconds);

        if (delta < 1 * MINUTE)
            return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

        if (delta < 2 * MINUTE)
            return "a minute ago";

        if (delta < 45 * MINUTE)
            return ts.Minutes + " minutes ago";

        if (delta < 90 * MINUTE)
            return "an hour ago";

        if (delta < 24 * HOUR)
            return ts.Hours + " hours ago";

        if (delta < 48 * HOUR)
            return "yesterday";

        if (delta < 30 * DAY)
            return ts.Days + " days ago";

        if (delta < 12 * MONTH)
        {
            int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
            return months <= 1 ? "one month ago" : months + " months ago";
        }
        else
        {
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }
    }

}