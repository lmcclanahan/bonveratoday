using System;


public static class DateTimeExtensions
{
    /// <summary>
    /// Gets the first week day following a date.
    /// </summary> 
    /// <param name="date">The date.</param> 
    /// <param name="dayOfWeek">The day of week to return.</param> 
    /// <returns>The first dayOfWeek day following date, or date if it is on dayOfWeek.</returns> 
    public static DateTime GetNextWeekDay(this DateTime date, DayOfWeek dayOfWeek)
    {
        return date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
    }

    ///<summary>Gets the first week day following a date.</summary>
    ///<param name="date">The date.</param>
    ///<param name="dayOfWeek">The day of week to return.</param>
    ///<returns>The first dayOfWeek day following date, or date if it is on dayOfWeek.</returns>
    public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek)
    {
        return date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
    }

    /// <summary>
    /// Get a DateTime that represents the beginning of the hour of the provided date.
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns></returns>
    public static DateTime BeginningOfHour(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0, 0, date.Kind);
    }

    /// <summary>
    /// Get a DateTime that represents the end of the hour of the provided date.
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns></returns>
    public static DateTime EndOfHour(this DateTime date)
    {
        return date.BeginningOfHour().AddHours(1).AddSeconds(-1).AddTicks(-1);
    }

    /// <summary>
    /// Get a DateTime that represents the beginning of the day of the provided date.
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns></returns>
    public static DateTime BeginningOfDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, date.Kind);
    }

    /// <summary>
    /// Get a DateTime that represents the end of the day of the provided date.
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns></returns>
    public static DateTime EndOfDay(this DateTime date)
    {
        return date.BeginningOfDay().AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Get a DateTime that represents the beginning of the month of the provided date.
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns></returns>
    public static DateTime BeginningOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1, 0, 0, 0, 0, date.Kind);
    }

    /// <summary>
    /// Get a DateTime that represents the end of the month of the provided date.
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns></returns>
    public static DateTime EndOfMonth(this DateTime date)
    {
        return date.BeginningOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Get a DateTime that represents the beginning of the year of the provided date.
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns></returns>
    public static DateTime BeginningOfYear(this DateTime date)
    {
        return new DateTime(date.Year, 1, 1, 0, 0, 0, 0, date.Kind);
    }

    /// <summary>
    /// Get a DateTime that represents the end of the year of the provided date.
    /// </summary>
    /// <param name="date">The date</param>
    /// <returns></returns>
    public static DateTime EndOfYear(this DateTime date)
    {
        return date.BeginningOfYear().AddYears(1).AddTicks(-1);
    }

    /// <summary>
    /// Determines if the provided datetime is a weekend.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static bool IsWeekend(this DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Extension method for dates to convert the date to CST
    /// </summary>
    /// <param name="dateToBeConverted"></param>
    /// <returns>Date in UTC</returns>
    public static DateTime ToCST(this DateTime dateToBeConverted)
    {

        var DateinUTC = TimeZoneInfo.ConvertTimeToUtc(dateToBeConverted);
        TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateinUTC, cstZone);
    }
}