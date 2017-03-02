using System;

namespace ExigoService
{
    public interface ICalendarEvent
    {
        Guid CalendarEventID { get; set; }
        Guid CalendarID { get; set; }
        int CalendarEventTypeID { get; set; }
        int CalendarEventStatusID { get; set; }
        int CalendarEventPrivacyTypeID { get; set; }
        int CreatedByCustomerID { get; set; }
        bool IsPrivateCopy { get; set; }

        string Description { get; set; }
        string Summary { get; set; }
        string Location { get; set; }
        string Url { get; set; }
        CalendarEventType CalendarEventType { get; set; }

        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        DateTime CreatedDate { get; set; }

        int? CalendarEventRecurrenceTypeID { get; set; }
        int? CalendarEventRecurrenceInterval { get; set; }
        int? CalendarEventRecurrenceMaxInstances { get; set; }
        DateTime? CalendarEventRecurrenceEndDate { get; set; }
        string WeeklyCalendarEventRecurrenceDays { get; set; }
        int? MonthlyCalendarEventRecurrenceTypeID { get; set; }

        string Field1 { get; set; }
        string Field2 { get; set; }
        string Field3 { get; set; }
        string Field4 { get; set; }
        string Field5 { get; set; }
        string Field6 { get; set; }
        string Field7 { get; set; }
        string Field8 { get; set; }
        string Field9 { get; set; }
        string Field10 { get; set; }
    }
}