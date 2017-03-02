using ExigoService;
using System.Linq;

namespace ReplicatedSite.Providers
{
    public class ODataIdentityAuthenticationProvider : Common.Providers.WebServiceIdentityAuthenticationProvider, IReplicatedSiteIdentityAuthenticationProvider
    {
        public ReplicatedSiteIdentity GetSiteOwnerIdentity(string webAlias)
        {
            return Exigo.OData().CustomerSites
                .Where(cs => cs.WebAlias == webAlias)
                .Select(c => new ReplicatedSiteIdentity
                {
                    CustomerID            = c.CustomerID,
                    CustomerTypeID        = c.Customer.CustomerTypeID,
                    CustomerStatusID      = c.Customer.CustomerStatusID,
                    HighestAchievedRankID = c.Customer.RankID,
                    CreatedDate           = c.Customer.CreatedDate,
                    WarehouseID           = c.Customer.DefaultWarehouseID,

                    WebAlias              = c.WebAlias,
                    FirstName             = c.FirstName,
                    LastName              = c.LastName,
                    Company               = c.Company,
                    Email                 = c.Email,
                    Phone                 = c.Phone,
                    Phone2                = c.Phone2,
                    Fax                   = c.Fax,

                    Address1              = c.Address1,
                    Address2              = c.Address2,
                    City                  = c.City,
                    State                 = c.State,
                    Zip                   = c.Zip,
                    Country               = c.Country,

                    Notes1                = c.Notes1,
                    Notes2                = c.Notes2,
                    Notes3                = c.Notes3,
                    Notes4                = c.Notes4
                })
                .FirstOrDefault();
        }
        public CustomerIdentity GetCustomerIdentity(int customerID)
        {
            return Exigo.OData().Customers
                .Where(c => c.CustomerID == customerID)
                .Select(c => new CustomerIdentity()
                {
                    CustomerID         = c.CustomerID,
                    FirstName          = c.FirstName,
                    LastName           = c.LastName,
                    Company            = c.Company,
                    LoginName          = c.LoginName,
                    CustomerTypeID     = c.CustomerTypeID,
                    CustomerStatusID   = c.CustomerStatusID,
                    LanguageID         = c.LanguageID,
                    DefaultWarehouseID = c.DefaultWarehouseID,
                    CurrencyCode       = c.CurrencyCode
                })
                .FirstOrDefault();
        }
    }
}
