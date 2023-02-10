using System;
using System.Threading;

namespace SSS.CommonLib.Entensions;

public static class DateTimeExtensions
{
    public static DateTimeOffset FirstDayOfWeek(this DateTimeOffset dt)
    {
        var culture = Thread.CurrentThread.CurrentCulture;

        var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;

        if (diff < 0)
        {
            diff += 7;
        }

        return dt.AddDays(-diff);
    }

    public static DateTime FirstDayOfWeek(this DateTime dt)
    {
        var culture = Thread.CurrentThread.CurrentCulture;

        var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;

        if (diff < 0)
        {
            diff += 7;
        }

        return dt.AddDays(-diff);
    }

    public static DateTime FirstDateOfWeek(int year, int weekOfYear)
    {
        var jan1 = new DateTime(year, 1, 1);
        int daysOffset = Convert.ToInt32(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) - Convert.ToInt32(jan1.DayOfWeek);
        var firstWeekDay = jan1.AddDays(daysOffset);
        System.Globalization.CultureInfo curCulture = System.Globalization.CultureInfo.CurrentCulture;
        int firstWeek = curCulture.Calendar.GetWeekOfYear(jan1, curCulture.DateTimeFormat.CalendarWeekRule, curCulture.DateTimeFormat.FirstDayOfWeek);
        if (firstWeek <= 1)
        {
            weekOfYear -= 1;
        }
        return firstWeekDay.AddDays(weekOfYear * 7);
    }

    public static DateTimeOffset LastDayOfWeek(this DateTimeOffset dt) =>
        dt.FirstDayOfWeek().AddDays(6);

    public static DateTimeOffset FirstDayOfMonth(this DateTimeOffset dt) =>
        new(new DateTime(dt.Year, dt.Month, 1), new TimeSpan(0, 0, 0));

    public static DateTime FirstDayOfMonth(this DateTime dt) =>
        new(dt.Year, dt.Month, 1);

    public static DateTime LastDayOfMonth(this DateTime dt) =>
        dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);

    public static DateTimeOffset LastDayOfMonth(this DateTimeOffset dt) =>
        dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);

    public static DateTimeOffset FirstDayOfNextMonth(this DateTimeOffset dt) =>
        dt.FirstDayOfMonth().AddMonths(1);

    public static string ToDateString(this DateTime dt, string format = "MM/dd/yyyy")
        => dt.ToString(format);
}