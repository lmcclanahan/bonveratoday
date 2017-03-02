using ExigoService;
using System.Collections.Generic;

namespace Common
{
    public class UnitedStatesMarket : Market
    {
        public UnitedStatesMarket()
            : base()
        {
            Name        = MarketName.UnitedStates;
            Description = "United States";
            CookieValue = "US";
            CultureCode = "en-US";
            IsDefault   = true;
            Countries   = new List<string> { "US" };

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
            return new UnitedStatesConfiguration();
        }
    }
}