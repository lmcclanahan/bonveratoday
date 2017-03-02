using Common;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace ReplicatedSite
{
    public static class Utilities
    {
        /// <summary>
        /// Gets the market the website is currently using.
        /// </summary>
        /// <returns>The Market object representing the current market.</returns>
        public static Market GetCurrentMarket()
        {
            var cultureCookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.SiteCultureCookieName];
            var cookieCountry = GlobalSettings.Company.Address.Country;
            if (cultureCookie != null)
            {
                cookieCountry = GlobalSettings.Markets.AvailableMarkets.Where(c => c.CultureCode == cultureCookie.Value).FirstOrDefault().CookieValue;
            }

            // Get the user's country to see which market we are in
            var country = (HttpContext.Current.Request.IsAuthenticated) ? Identity.Customer.Country : cookieCountry;
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
            var languageID = (HttpContext.Current.Request.IsAuthenticated) ? Identity.Customer.LanguageID : Languages.English;
            var language = GlobalSettings.Globalization.AvailableLanguages.Where(c => c.LanguageID == languageID).FirstOrDefault();

            // If we couldn't find the user's preferred language, return the first one we find.
            if (language == null) language = GlobalSettings.Globalization.AvailableLanguages.FirstOrDefault();

            // Return the language
            return language;
        }

        ///Evaluates if user is logged in so actual CustomerID can be used for methods like CalculateOrder can utilize.  Otherwise, just pass a zero so that Orders.cs does not give null exceptions
        public static int GetCustomerID()
        {
            return (Identity.Customer==null)? 0: Identity.Customer.CustomerID;
        }
    }
}