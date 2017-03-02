namespace Common.Api.ExigoOData.Calendars
{
    public partial class CalendarEventType
    {
        public static explicit operator ExigoService.CalendarEventType(CalendarEventType type)
        {
            var model = new ExigoService.CalendarEventType();
            if (type == null) return model;

            model.CalendarEventTypeID          = type.CalendarEventTypeID;
            model.CalendarEventTypeDescription = type.CalendarEventTypeDescription;
            model.Color                        = type.Color;

            return model;
        }
    }
}