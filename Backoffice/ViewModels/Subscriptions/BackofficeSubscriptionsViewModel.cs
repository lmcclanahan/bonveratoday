using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels
{
    public class BackofficeSubscriptionsViewModel
    {
        public BackofficeSubscriptionsViewModel()
        {
            this.Subscriptions = new List<CustomerSubscription>();
            this.SubscriptionItems = new List<Item>();
            this.AutoOrders = new List<AutoOrder>();
        }

        public List<CustomerSubscription> Subscriptions { get; set; }
        public IEnumerable<IPaymentMethod> PaymentMethods { get; set; }
        public OrderCalculationResponse OrderCalcResponse { get; set; }
        public List<Item> SubscriptionItems { get; set; }
        public List<AutoOrder> AutoOrders { get; set; }
    }
}