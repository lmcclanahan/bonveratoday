using System.Collections.Generic;
using System.Linq;

namespace ExigoService
{
    public static partial class Exigo
    {
        public static IEnumerable<PointAccount> GetPointAccounts()
        {
            var pointAccounts = Exigo.OData().PointAccounts.ToList();

            foreach (var pointAccount in pointAccounts)
            {
                yield return (ExigoService.PointAccount)pointAccount;
            }
        }
        public static PointAccount GetPointAccount(int pointAccountID)
        {
            var pointAccount = Exigo.OData().PointAccounts
                .Where(c => c.PointAccountID == pointAccountID)
                .FirstOrDefault();
            if (pointAccount == null) return null;

            return (ExigoService.PointAccount)pointAccount;
        }

        public static IEnumerable<CustomerPointAccount> GetCustomerPointAccounts(int customerID)
        {
            var pointAccounts = Exigo.OData().CustomerPointAccounts.Expand("PointAccount")
                .Where(c => c.CustomerID == customerID)
                .ToList();

            foreach (var pointAccount in pointAccounts)
            {
                yield return (ExigoService.CustomerPointAccount)pointAccount;
            }
        }
        public static CustomerPointAccount GetCustomerPointAccount(int customerID, int pointAccountID)
        {
            var pointAccount = Exigo.OData().CustomerPointAccounts.Expand("PointAccount")
                .Where(c => c.CustomerID == customerID)
                .Where(c => c.PointAccountID == pointAccountID)
                .FirstOrDefault();
            if (pointAccount == null) return null;

            return (ExigoService.CustomerPointAccount)pointAccount;
        }
        public static bool ValidateCustomerHasPointAmount(int customerID, int pointAccountID, decimal pointAmount)
        {
            var pointAccount = GetCustomerPointAccount(customerID, pointAccountID);
            if (pointAccount == null) return false;

            return pointAccount.Balance >= pointAmount;
        }
    }
}