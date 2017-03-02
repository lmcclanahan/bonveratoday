using System;

namespace Common.Api.ExigoOData
{
    public partial class CustomerSubscription
    {
        public static explicit operator ExigoService.CustomerSubscription(CustomerSubscription subscription)
        {
            var model = new ExigoService.CustomerSubscription();
            if (subscription == null) return model;

            model.SubscriptionID = subscription.SubscriptionID;
            model.CustomerID     = subscription.CustomerID;
            model.StartDate      = subscription.StartDate;
            model.ExpirationDate = subscription.ExpireDate;  
            model.IsExpired      = (subscription.Subscription != null) ? (subscription.SubscriptionStatus.SubscriptionStatusID != 1) : (model.ExpirationDate < DateTime.Now);

            if (subscription.Subscription != null)
            {
                model.SubscriptionDescription = subscription.Subscription.SubscriptionDescription;
            }

            return model;
        }
    }
}