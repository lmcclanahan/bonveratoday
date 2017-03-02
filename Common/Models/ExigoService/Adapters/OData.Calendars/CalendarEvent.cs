namespace Common.Api.ExigoOData.Calendars
{
    public partial class CalendarEvent
    {
        public static explicit operator ExigoService.CalendarEvent(CalendarEvent calendarEvent)
        {
            var model = new ExigoService.CalendarEvent();
            if (calendarEvent == null) return model;

            model.CalendarEventID                      = calendarEvent.CalendarEventID;
            model.CalendarID                           = calendarEvent.CalendarID;
            model.CalendarEventTypeID                  = calendarEvent.CalendarEventTypeID;
            model.CalendarEventStatusID                = calendarEvent.CalendarEventStatusID;
            model.CalendarEventPrivacyTypeID           = calendarEvent.CalendarEventPrivacyTypeID;
            model.CreatedByCustomerID                  = calendarEvent.CreatedByCustomerID;
            model.IsPrivateCopy                        = calendarEvent.IsPrivateCopy;

            model.Description                          = calendarEvent.Description;
            model.Summary                              = calendarEvent.Summary;
            model.Location                             = calendarEvent.Location;
            model.Url                                  = calendarEvent.Url;

            if (calendarEvent.CalendarEventType != null)
            {
                model.CalendarEventType = (ExigoService.CalendarEventType)calendarEvent.CalendarEventType;
            }

            model.UtcStartDate                         = calendarEvent.StartDate;
            model.UtcEndDate                           = calendarEvent.EndDate;
            model.CreatedDate                          = calendarEvent.CreatedDate;

            model.CalendarEventRecurrenceTypeID        = calendarEvent.CalendarEventRecurrenceTypeID;
            model.CalendarEventRecurrenceInterval      = calendarEvent.CalendarEventRecurrenceInterval;
            model.CalendarEventRecurrenceMaxInstances  = calendarEvent.CalendarEventRecurrenceMaxInstances;
            model.CalendarEventRecurrenceEndDate       = calendarEvent.CalendarEventRecurrenceEndDate;
            model.WeeklyCalendarEventRecurrenceDays    = calendarEvent.WeeklyCalendarEventRecurrenceDays;
            model.MonthlyCalendarEventRecurrenceTypeID = calendarEvent.MonthlyCalendarEventRecurrenceTypeID;

            model.Field1                               = calendarEvent.Field1;
            model.Field2                               = calendarEvent.Field2;
            model.Field3                               = calendarEvent.Field3;
            model.Field4                               = calendarEvent.Field4;
            model.Field5                               = calendarEvent.Field5;
            model.Field6                               = calendarEvent.Field6;
            model.Field7                               = calendarEvent.Field7;
            model.Field8                               = calendarEvent.Field8;
            model.Field9                               = calendarEvent.Field9;
            model.Field10                              = calendarEvent.Field10;

            return model;
        }
    }
}

namespace ExigoService
{
    public partial class CalendarEvent
    {
        public static explicit operator Common.Api.ExigoOData.Calendars.CalendarEvent(CalendarEvent calendarEvent)
        {
            var odatamodel = new Common.Api.ExigoOData.Calendars.CalendarEvent();
            if (calendarEvent == null) return odatamodel;

            odatamodel.CalendarEventID                      = calendarEvent.CalendarEventID;
            odatamodel.CalendarID                           = calendarEvent.CalendarID;
            odatamodel.CalendarEventTypeID                  = calendarEvent.CalendarEventTypeID;
            odatamodel.CalendarEventStatusID                = calendarEvent.CalendarEventStatusID;
            odatamodel.CalendarEventPrivacyTypeID           = calendarEvent.CalendarEventPrivacyTypeID;
            odatamodel.CreatedByCustomerID                  = calendarEvent.CreatedByCustomerID;
            odatamodel.IsPrivateCopy                        = calendarEvent.IsPrivateCopy;

            odatamodel.Description                          = calendarEvent.Description;
            odatamodel.Summary                              = calendarEvent.Summary;
            odatamodel.Location                             = calendarEvent.Location;
            odatamodel.Url                                  = calendarEvent.Url;

            odatamodel.StartDate                            = calendarEvent.UtcStartDate;
            odatamodel.EndDate                              = calendarEvent.UtcEndDate;
            odatamodel.CreatedDate                          = calendarEvent.CreatedDate;

            odatamodel.CalendarEventRecurrenceTypeID        = calendarEvent.CalendarEventRecurrenceTypeID;
            odatamodel.CalendarEventRecurrenceInterval      = calendarEvent.CalendarEventRecurrenceInterval;
            odatamodel.CalendarEventRecurrenceMaxInstances  = calendarEvent.CalendarEventRecurrenceMaxInstances;
            odatamodel.CalendarEventRecurrenceEndDate       = calendarEvent.CalendarEventRecurrenceEndDate;
            odatamodel.WeeklyCalendarEventRecurrenceDays    = calendarEvent.WeeklyCalendarEventRecurrenceDays;
            odatamodel.MonthlyCalendarEventRecurrenceTypeID = calendarEvent.MonthlyCalendarEventRecurrenceTypeID;

            odatamodel.Field1                               = calendarEvent.Field1;
            odatamodel.Field2                               = calendarEvent.Field2;
            odatamodel.Field3                               = calendarEvent.Field3;
            odatamodel.Field4                               = calendarEvent.Field4;
            odatamodel.Field5                               = calendarEvent.Field5;
            odatamodel.Field6                               = calendarEvent.Field6;
            odatamodel.Field7                               = calendarEvent.Field7;
            odatamodel.Field8                               = calendarEvent.Field8;
            odatamodel.Field9                               = calendarEvent.Field9;
            odatamodel.Field10                              = calendarEvent.Field10;

            return odatamodel;
        }
    }

}