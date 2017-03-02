namespace Common.Api.ExigoOData
{
    public partial class Customer
    {
        public static explicit operator ExigoService.CalendarSubscriptionCustomer(Customer customer)
        {
            var model = new ExigoService.CalendarSubscriptionCustomer();
            if (customer == null) return model;

            model.CustomerID = customer.CustomerID;
            model.FirstName = customer.FirstName;
            model.MiddleName = customer.MiddleName;
            model.LastName = customer.LastName;

            return model;
        }
    }
}