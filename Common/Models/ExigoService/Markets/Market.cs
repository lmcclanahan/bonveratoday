using Common;
using System.Collections.Generic;

namespace ExigoService
{
    public class Market : IMarket
    {
        public Market()
        {
            this.Configuration = GetConfiguration();
        }

        public MarketName Name { get; set; }
        public string Description { get; set; }
        public string CookieValue { get; set; }
        public string CultureCode { get; set; }
        public bool IsDefault { get; set; }
        public IEnumerable<string> Countries { get; set; }

        public List<IPaymentMethod> AvailablePaymentTypes { get; set; }
        public List<Common.Api.ExigoWebService.FrequencyType> AvailableAutoOrderFrequencyTypes { get; set; }

        // For Independent Associate enrollment
        public string RequiredEnrollmentPackItemCode { get; set; }

        // For Smart Shopper and Independent Associate Shopping
        public string FirstOrderPackItemCode { get; set; }
        public string SmartShopperFirstOrderPackItemCode { get; set; }

        // Virtual & Will Call Ship Methods
        public int VirtualShipMethodID { get; set; }
        public int WillCallShipMethodID { get; set; }

        public IMarketConfiguration Configuration { get; set; }
        public virtual IMarketConfiguration GetConfiguration()
        {
            return new UnitedStatesConfiguration();
        }
    }
}