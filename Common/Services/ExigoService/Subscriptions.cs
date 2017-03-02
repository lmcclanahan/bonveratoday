using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<Subscription> GetSubscriptions()
        {
            var subscriptions = Exigo.OData().Subscriptions.ToList();

            foreach (var subscription in subscriptions)
            {
                yield return (ExigoService.Subscription)subscription;
            }
        }
        public static Subscription GetSubscription(int subscriptionID)
        {
            var subscription = Exigo.OData().Subscriptions
                .Where(c => c.SubscriptionID == subscriptionID)
                .FirstOrDefault();
            if (subscription == null) return null;

            return (ExigoService.Subscription)subscription;
        }

        public static IEnumerable<CustomerSubscription> GetCustomerSubscriptions(int customerID)
        {
            var subscriptions = Exigo.OData().CustomerSubscriptions.Expand("Subscription,SubscriptionStatus")
                .ToList();

            foreach (var subscription in subscriptions)
            {
                yield return (ExigoService.CustomerSubscription)subscription;
            }
        }
        public static CustomerSubscription GetCustomerSubscription(int customerID, int subscriptionID)
        {
            var subscription = Exigo.OData().CustomerSubscriptions.Expand("Subscription,SubscriptionStatus")
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.SubscriptionID == subscriptionID)
                .FirstOrDefault();
            if (subscription == null) return null;

            return (ExigoService.CustomerSubscription)subscription;
        }
        public static bool ValidateCustomerHasSubscription(int customerID, int subscriptionID)
        {
            var subscription = GetCustomerSubscription(customerID, subscriptionID);
            if (subscription == null) return false;

            return !subscription.IsExpired;
        }
    }
}