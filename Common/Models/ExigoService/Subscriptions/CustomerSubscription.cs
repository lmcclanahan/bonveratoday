using System;

namespace ExigoService
{
    public class CustomerSubscription : Subscription
    {
        public int CustomerID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
