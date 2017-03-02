using ExigoService;
using System.Linq;
using Common;
using Dapper;

namespace ReplicatedSite.Providers
{
    public class SqlIdentityAuthenticationProvider : Common.Providers.SqlIdentityAuthenticationProvider, IReplicatedSiteIdentityAuthenticationProvider
    {
        public ReplicatedSiteIdentity GetSiteOwnerIdentity(string webAlias)
        {
            using (var context = Exigo.Sql())
            {
                return context.Query<ReplicatedSiteIdentity>(@"
                    SELECT
                        cs.CustomerID,
                        c.CustomerTypeID,
                        c.CustomerStatusID,
                        HighestAchievedRankID = c.RankID,
                        c.CreatedDate,
                        WarehouseID = COALESCE(c.DefaultWarehouseID, @defaultwarehouseid),

                        cs.WebAlias,
                        cs.FirstName,
                        cs.LastName,
                        cs.Company,
                        cs.Email,
                        cs.Phone,
                        cs.Phone2,
                        cs.Fax,

                        cs.Address1,
                        cs.Address2,
                        cs.City,
                        cs.State,
                        cs.Zip,
                        cs.Country,

                        cs.Notes1,
                        cs.Notes2,
                        cs.Notes3,
                        cs.Notes4

                    FROM CustomerSites cs
                        LEFT JOIN Customers c
                            ON c.CustomerID = cs.CustomerID
                    WHERE cs.webalias = @webalias
                ", new
                    {
                        webalias = webAlias,
                        defaultwarehouseid = Warehouses.Main
                    }).FirstOrDefault();
            }
        }
        public CustomerIdentity GetCustomerIdentity(int customerID)
        {
            using (var context = Exigo.Sql())
            {
                return context.Query<CustomerIdentity>(@"
                    SELECT
                        c.CustomerID,
                        c.FirstName,
                        c.LastName,
                        c.Company,
                        c.LoginName,
                        c.CustomerTypeID,
                        c.CustomerStatusID,
                        c.LanguageID,
                        DefaultWarehouseID = COALESCE(c.DefaultWarehouseID, @defaultwarehouseid),
                        c.CurrencyCode
                    FROM Customers c
                    WHERE c.CustomerID = @customerid
                ", new
                 {
                     customerid = customerID,
                     defaultwarehouseid = Warehouses.Main
                 }).FirstOrDefault();
            }
        }
    }
}
