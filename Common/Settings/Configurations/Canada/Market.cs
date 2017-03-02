using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class CanadaMarket : Market
    {
        public CanadaMarket()
            : base()
        {
            Name = MarketName.Canada;
            Description = "Canada";
            CookieValue = "CA";
            CultureCode = "en-US";
            Countries = new List<string> { "CA" };

            RequiredEnrollmentPackItemCode = "REGK";
            FirstOrderPackItemCode = "100000001";
            SmartShopperFirstOrderPackItemCode = "100000002";
            WillCallShipMethodID = 7;
            VirtualShipMethodID = 8;

            AvailablePaymentTypes = new List<IPaymentMethod>
            {
                new CreditCard()                
            };
            AvailableAutoOrderFrequencyTypes = new List<Common.Api.ExigoWebService.FrequencyType>
            {
                Common.Api.ExigoWebService.FrequencyType.Monthly                
            };
        }

        public override IMarketConfiguration GetConfiguration()
        {
            return new CanadaConfiguration();
        }
    }
}