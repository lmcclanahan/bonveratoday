using System;
using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public partial class CalendarEvent : ICalendarEvent
    {
        public Guid CalendarEventID { get; set; }
        public Guid CalendarID { get; set; }
        public int CalendarEventTypeID { get; set; }
        public int CalendarEventStatusID { get; set; }
        public int CalendarEventPrivacyTypeID { get; set; }
        public int CreatedByCustomerID { get; set; }
        public bool IsPrivateCopy { get; set; }
        public bool IsRecurringEventInstance { get; set; }

        public bool IsPersonal { get; set; }
        public bool IsEditable
        {
            get { return (this.IsPersonal || this.IsPrivateCopy); }
        }

        public string Description { get; set; }
        public string Summary { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
        public CalendarEventType CalendarEventType { get; set; }

        public DateTime UtcStartDate { get; set; }
        public DateTime StartDate
        {
            get { return TimeZoneInfo.ConvertTimeFromUtc(this.UtcStartDate, TimeZoneInfo.Local); }
            set { this.UtcStartDate = TimeZoneInfo.ConvertTimeToUtc(value, TimeZoneInfo.Local); }
        }
        public DateTime UtcEndDate { get; set; }
        public DateTime EndDate
        {
            get { return TimeZoneInfo.ConvertTimeFromUtc(this.UtcEndDate, TimeZoneInfo.Local); }
            set { this.UtcEndDate = TimeZoneInfo.ConvertTimeToUtc(value, TimeZoneInfo.Local); }
        }
        public DateTime CreatedDate { get; set; }

        public bool AllDay
        {
            get { return this.StartDate == this.StartDate.BeginningOfDay() && this.EndDate == this.EndDate.BeginningOfDay(); }
        }

        public int? CalendarEventRecurrenceTypeID { get; set; }
        public int? CalendarEventRecurrenceInterval { get; set; }
        public int? CalendarEventRecurrenceMaxInstances { get; set; }
        public DateTime? CalendarEventRecurrenceEndDate { get; set; }
        public string WeeklyCalendarEventRecurrenceDays { get; set; }
        public int? MonthlyCalendarEventRecurrenceTypeID { get; set; }

        public List<DayOfWeek> WeeklyCalendarEventRecurrenceDaysInWeek
        {
            get 
            {
                // Determine which days in the week this event should recur on.
                // If we don't have any specific days provided, use the start date's day of week.
                var repeatingDaysOfWeek = new List<DayOfWeek>();
                if (this.WeeklyCalendarEventRecurrenceDays.IsNotNullOrEmpty())
                {
                    var dayInWeekValues = this.WeeklyCalendarEventRecurrenceDays
                        .Split(',')
                        .Select(c => c.Trim());

                    foreach (var dayInWeekValue in dayInWeekValues)
                    {
                        repeatingDaysOfWeek.Add((DayOfWeek)Enum.Parse(typeof(DayOfWeek), dayInWeekValue));
                    }
                }
                else
                {
                    repeatingDaysOfWeek.Add(this.StartDate.DayOfWeek);
                }

                return repeatingDaysOfWeek;
            }
            set 
            {
                this.WeeklyCalendarEventRecurrenceDays = string.Join(",", value.Select(c => (int)c));
            }
        }

        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Field10 { get; set; }
    }
}