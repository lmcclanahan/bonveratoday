using ExigoService;
using System.Linq;
using System.Web;

namespace Common.Models
{
    public class IdentityUpline : IIdentityCacheable
    {
        public void Initialize(int customerID)
        {
            var upline = Exigo.OData().Customers
                .Where(c => c.CustomerID == customerID)
                .Select(c => new
                {
                    c.EnrollerID,
                    c.SponsorID
                }).FirstOrDefault();

            if (upline == null) return;

            if(upline.EnrollerID != null) this.Enroller = Exigo.GetCustomer((int)upline.EnrollerID);
            if (upline.SponsorID != null) this.Sponsor = Exigo.GetCustomer((int)upline.SponsorID);            
        }

        public string CacheKey { get; set; }
        public void RefreshCache()
        {
            HttpContext.Current.Cache.Remove(this.CacheKey);
        }

        public Customer Enroller { get; set; }
        public Customer Sponsor { get; set; }
    }
}