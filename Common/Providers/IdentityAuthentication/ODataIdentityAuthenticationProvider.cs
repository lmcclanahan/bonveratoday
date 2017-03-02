using ExigoService;
using System.Linq;

namespace Common.Providers
{
    public class ODataIdentityAuthenticationProvider : IIdentityAuthenticationProvider
    {
        public int AuthenticateCustomer(string loginname, string password)
        {
            var customer = (from c in Exigo.OData().CreateQuery<Common.Api.ExigoOData.Customer>("AuthenticateLogin")
                    .AddQueryOption("loginName", "'" + loginname + "'")
                    .AddQueryOption("password", "'" + password + "'")
                            select new Common.Api.ExigoOData.Customer { CustomerID = c.CustomerID }).FirstOrDefault();

            if (customer == null) return 0;
            else return customer.CustomerID;
        }
        public int AuthenticateCustomer(int customerid)
        {
            var customer = (from c in Exigo.OData().Customers
                            where c.CustomerID == customerid
                            select new { c.CustomerID })
                            .FirstOrDefault();

            if (customer == null) return 0;
            else return customer.CustomerID;
        }
    }
}
