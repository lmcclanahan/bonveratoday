using Common;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static Customer GetCustomerLead(int customerLeadID)
        {
            var customer = Exigo.OData().Customers.Expand("CustomerStatus")
                .Where(c => c.CustomerID == customerLeadID)
                .Where(c => c.CustomerTypeID == CustomerTypes.SmartShopper)
                .FirstOrDefault();
            if (customer == null) return null;

            return (Customer)customer;
        }
    }
}