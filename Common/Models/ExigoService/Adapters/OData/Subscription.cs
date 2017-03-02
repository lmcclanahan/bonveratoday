namespace Common.Api.ExigoOData
{
    public partial class Subscription
    {
        public static explicit operator ExigoService.Subscription(Subscription subscription)
        {
            var model = new ExigoService.Subscription();
            if (subscription == null) return model;

            model.SubscriptionID          = subscription.SubscriptionID;
            model.SubscriptionDescription = subscription.SubscriptionDescription;

            return model;
        }
    }
}