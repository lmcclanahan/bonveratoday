namespace Common.Api.ExigoOData.Calendars
{
    public partial class Customer
    {
        public static explicit operator ExigoService.Customer(Customer customer)
        {
            var model = new ExigoService.Customer();
            if (customer == null) return model;

            model.CustomerID          = customer.CustomerID;
            model.FirstName           = customer.FirstName;
            model.LastName            = customer.LastName;
            model.Company             = customer.Company;
            model.Email               = customer.Email;
            model.PrimaryPhone        = customer.Phone;

            return model;
        }
    }
}