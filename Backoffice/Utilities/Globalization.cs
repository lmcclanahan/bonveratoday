using Common;
using Common.Api.ExigoWebService;
using ExigoService;
using System.Linq;
using System.Web;

namespace Backoffice
{
    public static class Utilities
    {
        /// <summary>
        /// Gets the market the website is currently using.
        /// </summary>
        /// <returns>The Market object representing the current market.</returns>
        public static Market GetCurrentMarket()
        {
            // Get the user's country to see which market we are in
            var country = (HttpContext.Current.Request.IsAuthenticated) ? Identity.Current.Country : GlobalSettings.Company.Address.Country;
            var market = GlobalSettings.Markets.AvailableMarkets.Where(c => c.Countries.Contains(country)).FirstOrDefault();

            // If we didn't find a market for the user's country, get the first default market
            if (market == null) market = GlobalSettings.Markets.AvailableMarkets.Where(c => c.IsDefault == true).FirstOrDefault();

            // If we didn't find a default market, get the first market we find
            if (market == null) market = GlobalSettings.Markets.AvailableMarkets.FirstOrDefault();

            // Return the market
            return market;
        }

        /// <summary>
        /// Gets the language the user's preference is set to.
        /// </summary>
        /// <returns>The Language object representing the current user's language preference.</returns>
        public static Language GetCurrentLanguage()
        {
            // Get the user's language preference based on their saved preference
            var languageID = (HttpContext.Current.Request.IsAuthenticated) ? Identity.Current.LanguageID : Languages.English;
            var language = GlobalSettings.Globalization.AvailableLanguages.Where(c => c.LanguageID == languageID).FirstOrDefault();

            // If we couldn't find the user's preferred language, return the first one we find.
            if (language == null) language = GlobalSettings.Globalization.AvailableLanguages.FirstOrDefault();

            // Return the language
            return language;
        }


        public static string GetTranslatedAutoOrderPaymentMethodDescription(int paymentMethodID)
        {
            switch (paymentMethodID)
            {
                case (int)AutoOrderPaymentType.PrimaryCreditCard:
                    return Resources.Common.PrimaryCardOnFile;
                case (int)AutoOrderPaymentType.SecondaryCreditCard:
                    return Resources.Common.SecondaryCardOnFile;
                case (int)AutoOrderPaymentType.WillSendPayment:
                default:
                    return "";
            }
        }
    }
}