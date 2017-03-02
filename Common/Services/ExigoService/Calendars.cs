using System;
using System.Collections.Generic;
using System.Linq;
using CalendarContext = Common.Api.ExigoOData.Calendars;
using Common;
using System.Web;
using System.Web.Caching;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static ExigoService.Calendar CreateCalendar(CreateCalendarRequest request)
        {
            // Create the calendar
            var calendar = new CalendarContext.Calendar();
            calendar.CalendarID = Guid.NewGuid();
            calendar.CustomerID = request.CustomerID;
            calendar.Description = request.Description;
            calendar.CalendarTypeID = 1;
            calendar.CreatedDate = DateTime.Now;

            // Save the calendar
            var context = Exigo.ODataCalendars();
            context.AddToCalendars(calendar);
            context.SaveChanges();

            // Return the new calendar
            return (ExigoService.Calendar)calendar;
        }
        public static ExigoService.Calendar EnsureCalendar(int customerID)
        {
            // Get the provided customer's personal calendar count to see if they have one
            var calendars = GetCalendars(new GetCalendarsRequest
            {
                CustomerID = customerID,
                IncludeCalendarSubscriptions = false
            });

            // If we didn't find any calendars, create a default calendar and return it.
            /// Otherwise, return the first calendar we found.
            var calendar = new Calendar();

            if (calendars.Count() == 0)
            {
                calendar = CreateDefaultCalendar(customerID);
            }
            else
            {
                calendar = (ExigoService.Calendar)calendars.First();
            }

            // Ensure that we have at least one subscription, which is to our enroller
            var subscriptions = GetCustomerCalendarSubscriptions(customerID);
            if (subscriptions.Count() == 0)
            {
                CreateDefaultCalendarSubscriptions(customerID);
            }

            return calendar;
        }
        public static ExigoService.Calendar CreateDefaultCalendar(int customerID)
        {
            return CreateCalendar(new CreateCalendarRequest
            {
                CustomerID = customerID,
                Description = "Personal Calendar"
            });
        }
        public static void CreateDefaultCalendarSubscriptions(int customerID)
        {
            // Get their enroller ID
            var context = Exigo.OData();
            var enroller = context.Customers
                .Where(c => c.CustomerID == customerID)
                .Select(c => new
                {
                    c.EnrollerID
                })
                .FirstOrDefault();

            // If this person does not have an enroller, stop here.
            if (enroller == null || enroller.EnrollerID == null) return;

            // Get the enroller's default calendar
            var enrollerID = (int)enroller.EnrollerID;
            var calendars = GetCalendars(new GetCalendarsRequest
            {
                CustomerID = enrollerID,
                IncludeCalendarSubscriptions = false
            });

            // If they do not have a calendar, create them one
            var calendar = new Calendar();
            if (calendars.Count() == 0)
            {
                calendar = CreateDefaultCalendar(enrollerID);
            }
            else
            {
                calendar = calendars
                    .Where(c => c.CalendarTypeID == 1)
                    .OrderBy(c => c.CreatedDate)
                    .FirstOrDefault();
            }


            // Subscribe to the enroller's calendar
            SubscribeToCustomerCalendar(customerID, calendar.CalendarID);
        }
        public static IEnumerable<ExigoService.Calendar> GetCalendars(GetCalendarsRequest request)
        {
            var context = Exigo.ODataCalendars();
            var corporateCalendarContext = Exigo.OData();
            var calendars = new List<Calendar>();
            var cacheKey = "GetCalendars/{0}/{1}".FormatWith(request.CustomerID ?? 0, request.IncludeCalendarSubscriptions);
            var cache = (HttpContext.Current != null) ? HttpContext.Current.Cache : null;

            // Check the cache to see if we've already made this call recently
            if (cache == null || cache[cacheKey] == null)
            {
                GlobalUtilities.RunAsyncTasks(

                    // Add the customer's personal calendars
                    () =>
                    {
                        try
                        {
                            if (request.CustomerID != null)
                            {
                                var apiCalendars = context.Calendars
                                    .Where(c => c.CustomerID == (int)request.CustomerID)
                                    .ToList();
                                foreach (var apiCalendar in apiCalendars)
                                {
                                    calendars.Add((ExigoService.Calendar)apiCalendar);
                                }
                            }
                        }
                        catch { }
                    },

                    // Include the customer's calendar subscriptions if applicable
                    () =>
                    {
                        try
                        {
                            if (request.CustomerID != null && request.IncludeCalendarSubscriptions)
                            {
                                var calendarSubscriptions = GetCustomerCalendarSubscriptions((int)request.CustomerID);
                                var calendarSubscriptionsIDs = calendarSubscriptions.Select(c => c.CalendarID).ToList();

                                if (calendarSubscriptionsIDs.Count > 0)
                                {
                                    var apiCalendars = context.Calendars
                                        .Where(calendarSubscriptionsIDs.ToOrExpression<CalendarContext.Calendar, Guid>("CalendarID"))
                                        .ToList();
                                    foreach (var apiCalendar in apiCalendars)
                                    {
                                        calendars.Add((ExigoService.Calendar)apiCalendar);
                                    }
                                }
                            }
                        }
                        catch { }
                    },

                    // Include any additional requested calendars
                    () =>
                    {
                        try
                        {
                            if (request.CalendarIDs != null && request.CalendarIDs.Count() > 0)
                            {
                                var apiCalendars = context.Calendars
                                    .Where(request.CalendarIDs.ToList().ToOrExpression<CalendarContext.Calendar, Guid>("CalendarID"))
                                    .ToList();
                                foreach (var apiCalendar in apiCalendars)
                                {
                                    calendars.Add((ExigoService.Calendar)apiCalendar);
                                }
                            }
                        }
                        catch { }
                    }
                );


                // If we asked for a specific customer's calendars, and none of the calendars belong to that customer, create a default calendar and add it to the collection.
                if (request.CustomerID != null && calendars.Where(c => c.CustomerID == (int)request.CustomerID).Count() == 0)
                {
                    var defaultCalendar = CreateDefaultCalendar((int)request.CustomerID);
                    calendars.Add(defaultCalendar);
                }

                if (cache != null)
                {
                    cache.Insert(cacheKey, calendars,
                        null,
                        DateTime.Now.AddMinutes(5),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Normal,
                        null);
                }
            }
            else
            {
                calendars = (List<ExigoService.Calendar>)cache[cacheKey];
            }


            // Return the calendars
            foreach (var calendar in calendars)
            {
                yield return calendar;
            }
        }

        public static IEnumerable<ExigoService.Calendar> GetCustomerCalendarSubscriptions(int customerID)
        {
            var context = Exigo.ODataCalendars();
            var calendars = context.CalendarSubscriptions
                .Where(c => c.CustomerID == customerID)
                .Select(c => new
                {
                    c.Calendar
                })
                .ToList();

            foreach (var calendar in calendars)
            {
                yield return (ExigoService.Calendar)calendar.Calendar;
            }
        }
        public static void SubscribeToCustomerCalendar(int customerID, Guid calendarID)
        {
            // Check to ensure that the requested calendar exists
            var calendar = GetCalendars(new GetCalendarsRequest
            {
                CalendarIDs = new List<Guid>() { calendarID }
            }).FirstOrDefault();
            if (calendar == null) return;

            // Next, ensure the customer is not subscribing to their own calendar
            if (calendar.CustomerID == customerID) return;

            // Next, check to ensure the provided customer is not already subscribed to this calendar
            var existingSubscription = GetCustomerCalendarSubscriptions(customerID)
                .Where(c => c.CalendarID == calendarID)
                .FirstOrDefault();
            if (existingSubscription != null) return;

            // If we got here, we can go ahead and subscribe
            var context = Exigo.ODataCalendars();
            var newSubscription = new CalendarContext.CalendarSubscription();
            newSubscription.CustomerID = customerID;
            newSubscription.CalendarID = calendarID;

            context.AddToCalendarSubscriptions(newSubscription);
            context.SaveChanges();

            ClearCalendarCache("GetCalendars/{0}/{1}", customerID, true);
        }
        public static void UnsubscribeFromCustomerCalendar(int customerID, Guid calendarID)
        {
            // Grab the customer's existing subscription and delete it if it exists
            var context = Exigo.ODataCalendars();
            var existingSubscription = context.CalendarSubscriptions
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.CalendarID == calendarID)
                .FirstOrDefault();
            if (existingSubscription != null)
            {
                context.DeleteObject(existingSubscription);
                context.SaveChanges();

                ClearCalendarCache("GetCalendars/{0}/{1}", customerID, true);
            }
        }

        public static ExigoService.CalendarEvent GetCalendarEvent(Guid calendarEventID)
        {
            // Get the requested calendar event
            var context = Exigo.ODataCalendars();
            var calendarEvent = context.CalendarEvents.Expand("CalendarEventType")
                .Where(c => c.CalendarEventID == calendarEventID)
                .FirstOrDefault();

            return (ExigoService.CalendarEvent)calendarEvent;
        }
        public static IEnumerable<ExigoService.CalendarEvent> GetCalendarEvents(GetCalendarEventsRequest request)
        {
            // Get the customer's calendars
            var calendars = GetCalendars(request);
            var calendarIDs = calendars.Select(c => c.CalendarID).ToList();


            // Create the collection of events
            var calendarEvents = new List<ExigoService.CalendarEvent>();


            // First, grab the single events that fall between those dates
            var context = Exigo.ODataCalendars();
            var singleEvents = context.CalendarEvents.Expand("CalendarEventType")
                .Where(calendarIDs.ToOrExpression<CalendarContext.CalendarEvent, Guid>("CalendarID"))
                .Where(c => c.CalendarEventRecurrenceTypeID == null)
                .Where(c => c.StartDate >= request.UtcStartDate && c.StartDate <= request.UtcEndDate)
                .Where(c => c.EndDate >= request.UtcStartDate && c.EndDate <= request.UtcEndDate)
                .ToList();


            // Convert the single events to our model
            foreach (var singleEvent in singleEvents)
            {
                var modelSingleEvent = (ExigoService.CalendarEvent)singleEvent;
                calendarEvents.Add(modelSingleEvent);
            }


            // Next, grab the recurring events
            var recurringEvents = context.CalendarEvents.Expand("CalendarEventType")
                .Where(calendarIDs.ToOrExpression<CalendarContext.CalendarEvent, Guid>("CalendarID"))
                .Where(c => c.CalendarEventRecurrenceTypeID != null)
                .Where(c => c.StartDate <= request.UtcEndDate)
                .ToList();

            foreach (var recurringEvent in recurringEvents)
            {
                var instances = new List<ExigoService.CalendarEvent>();
                var modelRecurringEvent = (ExigoService.CalendarEvent)recurringEvent;

                instances = GetCalendarRecurringEventInstances(instances, modelRecurringEvent, request).ToList();
                foreach (var instance in instances)
                {
                    calendarEvents.Add(instance);
                }
            }


            // Order and return the events in the collection
            calendarEvents = calendarEvents.OrderBy(c => c.StartDate).ToList();
            foreach (var calendarEvent in calendarEvents)
            {
                yield return calendarEvent;
            }
        }
        private static IEnumerable<ExigoService.CalendarEvent> GetCalendarRecurringEventInstances(List<ExigoService.CalendarEvent> instances, ExigoService.CalendarEvent recurringEvent, GetCalendarEventsRequest request, int instanceAttempts = 0)
        {
            // Set some variables for easier access
            if (recurringEvent.CalendarEventRecurrenceTypeID == null) return instances;
            var recurrenceTypeID = (int)recurringEvent.CalendarEventRecurrenceTypeID;
            var recurrenceInterval = (int)recurringEvent.CalendarEventRecurrenceInterval;


            // Before we do anything, validate that our start date is valid based on its recurrence settings
            // Weekly recurrence validations
            if (recurrenceTypeID == 2)
            {
                var indexInList = recurringEvent.WeeklyCalendarEventRecurrenceDaysInWeek.IndexOf(recurringEvent.StartDate.DayOfWeek);

                // If the start date's day of week doesn't match any of the available days in the week, 
                // we need to make an adjustment to the start date and restart the method.
                if (indexInList == -1)
                {
                    instanceAttempts--;
                    var daysToAdjust = 0;

                    foreach (var day in recurringEvent.WeeklyCalendarEventRecurrenceDaysInWeek)
                    {
                        if ((int)day > (int)recurringEvent.StartDate.DayOfWeek)
                        {
                            daysToAdjust = (int)day - (int)recurringEvent.StartDate.DayOfWeek;
                            recurringEvent.StartDate = recurringEvent.StartDate.AddDays(daysToAdjust);
                            recurringEvent.EndDate = recurringEvent.EndDate.AddDays(daysToAdjust);
                            break;
                        }
                    }

                    // If we didn't find a day in the week that follows the current day, advance to the first available day in the next week
                    if (daysToAdjust == 0)
                    {
                        daysToAdjust = 7 - ((int)recurringEvent.StartDate.DayOfWeek - (int)recurringEvent.WeeklyCalendarEventRecurrenceDaysInWeek.First());
                        recurringEvent.StartDate = recurringEvent.StartDate.AddDays(daysToAdjust);
                        recurringEvent.EndDate = recurringEvent.EndDate.AddDays(daysToAdjust);
                    }
                    return GetCalendarRecurringEventInstances(instances, recurringEvent, request, instanceAttempts);
                }
            }


            // Add an instance of this recurring event if applicable
            if (recurringEvent.StartDate >= request.StartDate && recurringEvent.StartDate < request.EndDate)
            {
                // Create and add the instance
                var instance = GlobalUtilities.Extend(recurringEvent, new ExigoService.CalendarEvent());
                instance.IsRecurringEventInstance = true;
                instances.Add(instance);
            }


            // Increment the amount of instances attempted
            instanceAttempts++;


            // If we have the maximum number of instances, stop creating instances and return the collection
            if (recurringEvent.CalendarEventRecurrenceMaxInstances != null && instanceAttempts >= (int)recurringEvent.CalendarEventRecurrenceMaxInstances)
            {
                return instances;
            }


            // Increment the period of time based on the recurrence type.
            switch (recurrenceTypeID)
            {
                // Daily
                case 1:
                    recurringEvent.StartDate = recurringEvent.StartDate.AddDays(1 * recurrenceInterval);
                    recurringEvent.EndDate = recurringEvent.EndDate.AddDays(1 * recurrenceInterval);
                    break;

                // Weekly
                case 2:
                    // Determine if we are repeating this event more than once per week.
                    // If this event only occurs once per week, just add another week.
                    if (recurringEvent.WeeklyCalendarEventRecurrenceDaysInWeek.Count == 1)
                    {
                        recurringEvent.StartDate = recurringEvent.StartDate.AddDays(7 * recurrenceInterval);
                        recurringEvent.EndDate = recurringEvent.EndDate.AddDays(7 * recurrenceInterval);
                    }

                    // If this event occurs more than once in a week, let's ensure that we advance the dates appropriately.
                    else
                    {
                        var indexInList = recurringEvent.WeeklyCalendarEventRecurrenceDaysInWeek.IndexOf(recurringEvent.UtcStartDate.DayOfWeek);


                        // If we are on the last day in the list, skip ahead to the front of the list
                        var daysToAdvance = 0;
                        if (indexInList == (recurringEvent.WeeklyCalendarEventRecurrenceDaysInWeek.Count - 1))
                        {
                            daysToAdvance = (7 - ((int)recurringEvent.StartDate.DayOfWeek - (int)recurringEvent.WeeklyCalendarEventRecurrenceDaysInWeek.First())) + (7 * (recurrenceInterval - 1));
                        }
                        // Otherwise, add the number of days between the current start date and the next day in the week.
                        else
                        {
                            daysToAdvance = ((int)recurringEvent.WeeklyCalendarEventRecurrenceDaysInWeek[indexInList + 1]) - ((int)recurringEvent.WeeklyCalendarEventRecurrenceDaysInWeek[indexInList]);
                        }

                        recurringEvent.StartDate = recurringEvent.StartDate.AddDays(daysToAdvance);
                        recurringEvent.EndDate = recurringEvent.EndDate.AddDays(daysToAdvance);
                    }
                    break;

                // Monthly
                case 3:
                    var monthlyRecurringTypeID = (int)recurringEvent.MonthlyCalendarEventRecurrenceTypeID;
                    switch (monthlyRecurringTypeID)
                    {
                        // Day of the month
                        case 1:
                            recurringEvent.StartDate = recurringEvent.StartDate.AddMonths(1 * recurrenceInterval);
                            recurringEvent.EndDate = recurringEvent.EndDate.AddMonths(1 * recurrenceInterval);
                            break;

                        // Day of the week (ie. second Tuesday of the month)
                        case 2:
                            // Determine which week and day the current start date is in 
                            var dayOfWeek = recurringEvent.StartDate.DayOfWeek;
                            var nthWeek = 1;
                            var checkingDate = recurringEvent.StartDate.BeginningOfMonth().Next(dayOfWeek);
                            while (recurringEvent.StartDate != checkingDate)
                            {
                                checkingDate.AddDays(7);
                                nthWeek++;
                            }

                            var nextMonth = recurringEvent.StartDate.AddMonths(1 * recurrenceInterval).BeginningOfMonth();
                            var timeDifference = recurringEvent.EndDate.Subtract(recurringEvent.StartDate);

                            recurringEvent.StartDate = GlobalUtilities.GetNthWeekofMonth(nextMonth, nthWeek, dayOfWeek);
                            recurringEvent.EndDate = recurringEvent.StartDate.Add(timeDifference);
                            break;
                    }
                    break;

                // Yearly
                case 4:
                    recurringEvent.StartDate = recurringEvent.StartDate.AddYears(1 * recurrenceInterval);
                    recurringEvent.EndDate = recurringEvent.EndDate.AddYears(1 * recurrenceInterval);
                    break;
            }


            // If we have exceeded the maximum recurrence end date, stop creating instances and return the collection
            if (recurringEvent.CalendarEventRecurrenceEndDate != null && (DateTime)recurringEvent.CalendarEventRecurrenceEndDate < recurringEvent.StartDate)
            {
                return instances;
            }

            // If our new start date has exceeded the range of the original request, stop creating instances and return the collection
            if (recurringEvent.StartDate > request.EndDate)
            {
                return instances;
            }


            return GetCalendarRecurringEventInstances(instances, recurringEvent, request, instanceAttempts);
        }

        public static ExigoService.CalendarEvent SaveCalendarEvent(ExigoService.CalendarEvent model)
        {
            // Create our context
            var context = Exigo.ODataCalendars();

            // Convert our model to the event
            var odataCalendarEvent = (CalendarContext.CalendarEvent)model;

            // Check to see if we are creating a new event, or changing an existing
            if (odataCalendarEvent.CalendarEventID == Guid.Empty)
            {
                odataCalendarEvent.CalendarEventID = Guid.NewGuid();
                context.AddToCalendarEvents(odataCalendarEvent);
            }
            else
            {
                context.AttachTo("CalendarEvents", odataCalendarEvent);
                context.UpdateObject(odataCalendarEvent);
            }

            // Save the event
            context.SaveChanges();

            // Set the new calendar event ID
            model.CalendarEventID = odataCalendarEvent.CalendarEventID;

            return model;
        }
        public static ExigoService.CalendarEvent CopyCalendarEvent(Guid calendarEventID, int customerID, bool copyRecurrenceSettings = true)
        {
            // Get the event
            var context = Exigo.ODataCalendars();
            var calendarEvent = context.CalendarEvents
                .Where(c => c.CalendarEventID == calendarEventID)
                .FirstOrDefault();
            if (calendarEvent == null) return null;


            // Create a clone of the event
            var copiedCalendarEvent = GlobalUtilities.Extend(calendarEvent, new CalendarContext.CalendarEvent());


            // Fetch the calendars
            var calendar = GetCalendars(new GetCalendarsRequest
            {
                CustomerID = customerID,
                IncludeCalendarSubscriptions = false
            }).FirstOrDefault();
            if (calendar == null) return null;


            // Change some settings to give the event new ownership
            copiedCalendarEvent.CalendarID = calendar.CalendarID;
            copiedCalendarEvent.CalendarEventID = Guid.Empty;
            copiedCalendarEvent.IsPrivateCopy = true;

            // Clear out the recurring settings if desired
            if (!copyRecurrenceSettings)
            {
                copiedCalendarEvent.CalendarEventRecurrenceTypeID = null;
                copiedCalendarEvent.CalendarEventRecurrenceInterval = null;
                copiedCalendarEvent.CalendarEventRecurrenceMaxInstances = null;
                copiedCalendarEvent.CalendarEventRecurrenceEndDate = null;
                copiedCalendarEvent.WeeklyCalendarEventRecurrenceDays = null;
                copiedCalendarEvent.MonthlyCalendarEventRecurrenceTypeID = null;
            }


            return (ExigoService.CalendarEvent)copiedCalendarEvent;
        }
        public static void DeleteCalendarEvent(Guid calendarEventID, int customerID)
        {
            // Get the event
            var context = Exigo.ODataCalendars();
            var calendarEvent = context.CalendarEvents
                .Where(c => c.Calendar.CustomerID == customerID)
                .Where(c => c.CalendarEventID == calendarEventID)
                .FirstOrDefault();
            if (calendarEvent == null) return;

            // Delete the event
            context.DeleteObject(calendarEvent);
            context.SaveChanges();
        }

        public static IEnumerable<ExigoService.CalendarEventType> GetCalendarEventTypes()
        {
            // Get all calendar event types
            var context = Exigo.ODataCalendars();
            var calendarEventTypes = context.CalendarEventTypes.ToList();

            foreach (var calendarEventType in calendarEventTypes)
            {
                yield return (ExigoService.CalendarEventType)calendarEventType;
            }
        }

        public static IEnumerable<TimeZoneInfo> GetTimeZones()
        {
            var zones = TimeZoneInfo.GetSystemTimeZones();
            var timezones = new List<TimeZoneInfo>();

            foreach (var zone in zones)
            {
                timezones.Add(zone);
            }

            return timezones;
        }



        private static void ClearCalendarCache(string cacheKeyFormat, params object[] arguments)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Cache.Remove(string.Format(cacheKeyFormat, arguments));
            }
        }
    }
}