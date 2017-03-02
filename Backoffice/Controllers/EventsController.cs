using Backoffice.ViewModels;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Backoffice.Controllers
{
    [RoutePrefix("events")]
    public class EventsController : Controller
    {
        [Route("")]
        public ActionResult Calendar()
        {
            ViewBag.Calendars = Exigo.GetCalendars(new GetCalendarsRequest
            {
                CustomerID = Identity.Current.CustomerID,
                IncludeCalendarSubscriptions = true
            });


            // Get the event types (for the legend)
            ViewBag.CalendarEventTypes = Exigo.GetCalendarEventTypes().ToList();


            return View();
        }

        public ActionResult Subscriptions()
        {
            var model = new SubscriptionsViewModel();

            // Get the calendars we are subscribed to
            var calendars = Exigo.GetCustomerCalendarSubscriptions(Identity.Current.CustomerID);


            // Get the customers for each of the calendar subscriptions
            var customerIDs = calendars.Select(c => c.CustomerID).Distinct().ToList();
            var apiCustomers = Exigo.OData().Customers
                .Where(customerIDs.ToOrExpression<Common.Api.ExigoOData.Customer, int>("CustomerID"))
                .Select(c => new Common.Api.ExigoOData.Customer()
                {
                    CustomerID = c.CustomerID,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                })
                .ToList();


            // Pair the data together to create our models
            var customers = new List<CalendarSubscriptionCustomer>();
            foreach (var apiCustomer in apiCustomers)
            {
                // Create the special customer first
                var customer = new CalendarSubscriptionCustomer()
                {
                    CustomerID = apiCustomer.CustomerID,
                    FirstName = apiCustomer.FirstName,
                    LastName = apiCustomer.LastName
                };

                // Add their calendars
                customer.Calendars = calendars.Where(c => c.CustomerID == customer.CustomerID).ToList();

                // Add our model to the collection
                model.CalendarSubscriptionCustomers.Add(customer);
            }


            return View(model);
        }

        [HttpPost]
        public ActionResult GetSubscriptions()
        {
            var subscriptions = Exigo.GetCustomerCalendarSubscriptions(Identity.Current.CustomerID);

            var ids = new List<string>();

            foreach (var sub in subscriptions)
            {
                var id = sub.CalendarID.ToString();

                ids.Add(id);
            }

            return new JsonNetResult(new
            {
                success = true,
                subscriptions = ids.ToArray()
            });
        }

        [HttpPost]
        public ActionResult SearchSubscriptions(string query)
        {
            try
            {
                // assemble a list of customers who match the search criteria
                var apiCalendars = new List<Common.Api.ExigoOData.Calendars.Calendar>();


                var basequery = Exigo.ODataCalendars().Calendars.Expand("Customer").AsQueryable();

                var isCustomerID = query.CanBeParsedAs<int>();
                if (isCustomerID)
                {
                    basequery = basequery
                    .Where(c => c.CustomerID == Convert.ToInt32(query))
                    .Where(c => c.CustomerID != Identity.Current.CustomerID);
                }
                else
                {
                    basequery = basequery.Where(c => c.Customer.FirstName.Contains(query) || c.Customer.LastName.Contains(query));
                }


                if (basequery.Count() > 0)
                {
                    apiCalendars = basequery.Select(c => c).ToList();
                }



                // Pair the data together to create our models
                var calendars = new List<Calendar>();
                foreach (var apiCalendar in apiCalendars)
                {
                    calendars.Add((ExigoService.Calendar)apiCalendar);
                }


                var results = calendars.GroupBy(c => c.Customer, (customer, cals) => new CalendarSubscriptionCustomer
                {
                    CustomerID = customer.CustomerID,
                    FirstName  = customer.FirstName,
                    LastName   = customer.LastName,
                    Calendars  = cals.ToList()
                }).ToList();



                return new JsonNetResult(new
                {
                    success = true,
                    subscriptions = results
                });
            }
            catch (Exception ex)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public ActionResult SubscribeToDistributorCalendar(Guid id)
        {
            Exigo.SubscribeToCustomerCalendar(Identity.Current.CustomerID, id);

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    success = true
                });
            }
            else return RedirectToAction("subscriptions");
        }

        [HttpPost]
        public ActionResult UnsubscribeFromDistributorCalendar(Guid id)
        {
            Exigo.UnsubscribeFromCustomerCalendar(Identity.Current.CustomerID, id);

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    success = true
                });
            }
            else return RedirectToAction("subscriptions");
        }

        [HttpPost]
        public JsonNetResult GetEvents(DateTime start, DateTime end)
        {
            var events = Exigo.GetCalendarEvents(new GetCalendarEventsRequest
            {
                CustomerID                   = Identity.Current.CustomerID,
                IncludeCalendarSubscriptions = true,
                StartDate                    = start,
                EndDate                      = end
            }).ToList();

            foreach (var e in events)
            {
                e.IsPersonal = (e.CreatedByCustomerID == Identity.Current.CustomerID || e.IsPrivateCopy);
            }

            return new JsonNetResult(new
            {
                success = true,
                events = events
            });
        }

        [Route("agenda")]
        public ActionResult Agenda()
        {
            ViewBag.StartDate = DateTime.Now;
            ViewBag.EndDate = DateTime.Now.AddDays(7);

            return View();
        }

        [HttpPost]
        public ActionResult GetAgendaEventList(DateTime startDate, DateTime endDate)
        {
            var events = Exigo.GetCalendarEvents(new GetCalendarEventsRequest()
            {
                CustomerID = Identity.Current.CustomerID,
                IncludeCalendarSubscriptions = true,
                StartDate = startDate,
                EndDate = endDate
            });


            if (Request.IsAjaxRequest()) return PartialView("Partials/_AgendaEventList", events);
            else return View("Partials/_AgendaEventList", events);
        }

        #region Creating/Editing Events
        [Route("new")]
        public ActionResult CreateEvent()
        {
            // Create the new event model with some defaults
            var model                        = new CalendarEvent();
            model.CreatedDate                = DateTime.Now;
            model.StartDate                  = DateTime.Now.AddHours(1).BeginningOfHour();
            model.EndDate                    = model.StartDate.AddHours(1);
            model.CalendarEventStatusID      = 1;
            model.CalendarEventPrivacyTypeID = 1;
            model.CalendarEventTypeID        = 1;
            model.CreatedByCustomerID        = Identity.Current.CustomerID;

            return ManageEvent(model);
        }

        [Route("edit/{eventid}")]
        public ActionResult EditEvent(Guid eventid)
        {
            // Get the calendar event
            var model = Exigo.GetCalendarEvent(eventid);

            return ManageEvent(model);
        }

        public ActionResult ManageEvent(CalendarEvent calendarEvent)
        {
            // Get all available timezones
            ViewBag.TimeZones = Exigo.GetTimeZones();


            // Get the customer's personal calendars
            var calendars = Exigo.GetCalendars(new GetCalendarsRequest
            {
                CustomerID                   = Identity.Current.CustomerID,
                IncludeCalendarSubscriptions = false
            }).ToList();
            ViewBag.Calendars = calendars;


            // Get the event types
            ViewBag.CalendarEventTypes = Exigo.GetCalendarEventTypes().ToList();


            // Set some defaults for new events now that we have the calendars
            if (calendarEvent.CalendarID == Guid.Empty) calendarEvent.CalendarID = calendars.First().CalendarID;


            // Return the view
            if (Request.IsAjaxRequest()) return PartialView("ManageEvent", calendarEvent);
            else return View("ManageEvent", calendarEvent);
        }

        [HttpPost]
        public JsonNetResult SaveEvent(CalendarEvent calendarEvent)
        {
            try
            {
                calendarEvent = Exigo.SaveCalendarEvent(calendarEvent);

                return new JsonNetResult(new
                {
                    success = true,
                    calendareventid = calendarEvent.CalendarEventID
                });
            }
            catch (Exception exception)
            {
                return new JsonNetResult(new
                {
                    success = false,
                    message = exception.Message
                });
            }

        }

        [Route("copy/{eventid}")]
        public ActionResult CopyEvent(Guid eventid)
        {
            var calendarEvent = Exigo.CopyCalendarEvent(eventid, Identity.Current.CustomerID);

            return ManageEvent(calendarEvent);
        }

        [Route("delete/{eventid}")]
        public ActionResult DeleteEvent(Guid eventid)
        {
            Exigo.DeleteCalendarEvent(eventid, Identity.Current.CustomerID);

            return RedirectToAction("Calendar");
        }
        #endregion

        #region Models and Enums
        public class SearchResult
        {
            public int CustomerID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName
            {
                get { return this.FirstName + " " + this.LastName; }
            }
            public string AvatarURL { get; set; }
        }
        #endregion
    }
}