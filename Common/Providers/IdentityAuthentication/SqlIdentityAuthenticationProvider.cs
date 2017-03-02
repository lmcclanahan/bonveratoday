using Common.Helpers;

namespace Common.Providers
{
    public class SqlIdentityAuthenticationProvider : IIdentityAuthenticationProvider
    {
        public int AuthenticateCustomer(string loginname, string password)
        {
            var command = new SqlHelper();
            var customerID = command.GetField("AuthenticateCustomer {0}, {1}", loginname, password);

            if (customerID == null) return 0;
            else return (int)customerID;
        }
        public int AuthenticateCustomer(int customerid)
        {
            var command = new SqlHelper();
            var customerID = command.GetField("select CustomerID from Customers where CustomerID = {0}", customerid);

            if (customerID == null) return 0;
            else return (int)customerID;
        }
    }
}