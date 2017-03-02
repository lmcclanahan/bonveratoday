using Common.Api.ExigoWebService;
using System;

namespace Common
{
    public static partial class GlobalUtilities
    {
        /// <summary>
        /// Gets the start date for an autoOrders with the provided frequency type.
        /// </summary>
        /// <param name="frequency">How often the autoOrder will run</param>
        /// <returns>The start date for an autoOrder</returns>
        public static DateTime GetAutoOrderStartDate(FrequencyType frequency)
        {
            DateTime autoOrderStartDate = new DateTime();
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            switch (frequency)
            {
                case FrequencyType.Weekly: autoOrderStartDate = now.AddDays(7); break;
                case FrequencyType.BiWeekly: autoOrderStartDate = now.AddDays(14); break;
                case FrequencyType.EveryFourWeeks: autoOrderStartDate = now.AddDays(28); break;
                case FrequencyType.Monthly: autoOrderStartDate = now.AddMonths(1); break;
                case FrequencyType.BiMonthly: autoOrderStartDate = now.AddMonths(2); break;
                case FrequencyType.Quarterly: autoOrderStartDate = now.AddMonths(3); break;
                case FrequencyType.SemiYearly: autoOrderStartDate = now.AddMonths(6); break;
                case FrequencyType.Yearly: autoOrderStartDate = now.AddYears(1); break;
                default: autoOrderStartDate = now; break;
            }

            // Ensure we are not returning a day of 29, 30 or 31.
            autoOrderStartDate = GetNextAvailableAutoOrderStartDate(autoOrderStartDate);

            return autoOrderStartDate;
        }

        /// <summary>
        /// Gets the next available date for an autoOrder starting with the provided date.
        /// </summary>
        /// <param name="date">The original start date</param>
        /// <returns>The nearest available start date for an autoOrder</returns>
        public static DateTime GetNextAvailableAutoOrderStartDate(DateTime date)
        {           
            if (date.Day > 25)
            {
                date = new DateTime(date.AddMonths(1).Year, date.AddMonths(1).Month, 1).Date;
            }

            return date;
        }
    }
}