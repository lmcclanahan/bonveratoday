namespace ExigoService
{
    public interface ICalendarEventType
    {
        int CalendarEventTypeID { get; set; }
        string CalendarEventTypeDescription { get; set; }
        string Color { get; set; }
    }
}