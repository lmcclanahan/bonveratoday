using System;

namespace ExigoService
{
    public class GetCalendarEventsRequest : GetCalendarsRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime UtcStartDate
        {
            get { return TimeZoneInfo.ConvertTimeToUtc(this.StartDate, TimeZoneInfo.Local); }
        }
        public DateTime UtcEndDate
        {
            get { return TimeZoneInfo.ConvertTimeToUtc(this.EndDate, TimeZoneInfo.Local); }
        }
    }
}