using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels
{
    public class SubscriptionsViewModel
    {
        public SubscriptionsViewModel()
        {
            this.CalendarSubscriptionCustomers = new List<CalendarSubscriptionCustomer>();
        }

        public List<CalendarSubscriptionCustomer> CalendarSubscriptionCustomers { get; set; }
    }
}