using ExigoService;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;

namespace Common
{
    public static partial class GlobalUtilities
    {
        /// <summary>
        /// Sets the CultureCode of the site based on the current market.
        /// </summary>
        public static void SetCurrentCulture(string cultureCode)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureCode);
        }

        /// <summary>
        /// Get the selected country code 
        /// </summary>
        /// <returns></returns>
        public static string GetSelectedCountryCode(string countryCode = "US")
        {
            var cookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.CountryCookieName];

            if (countryCode != "US")
            {
                cookie = new HttpCookie(GlobalSettings.Globalization.CountryCookieName);
                cookie.Value = countryCode;
                HttpContext.Current.Response.Cookies.Add(cookie);

                return countryCode;
            }

            if (cookie != null && !cookie.Value.IsEmpty())
            {
                return cookie.Value;
            }
            else
            {
                cookie = new HttpCookie(GlobalSettings.Globalization.CountryCookieName);
                cookie.Value = countryCode;

                HttpContext.Current.Response.Cookies.Add(cookie);

                return cookie.Value;
            }
        }

        public static string SetSelectedCountryCode(string countryCode)
        {
            var cookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.CountryCookieName];

            if (cookie != null && !cookie.Value.IsEmpty())
            {
                cookie.Value = countryCode;
                HttpContext.Current.Response.Cookies.Add(cookie);

                return cookie.Value;
            }
            else
            {
                cookie = new HttpCookie(GlobalSettings.Globalization.CountryCookieName);
                cookie.Value = countryCode;

                HttpContext.Current.Response.Cookies.Add(cookie);

                return cookie.Value;
            }
        }

        /// <summary>
        /// Gets the configuration, based on the country cookie
        /// </summary>
        public static IMarketConfiguration GetCurrentMarketConfiguration()
        {
            var countryCode = GlobalUtilities.GetSelectedCountryCode();

            return GlobalSettings.Markets.AvailableMarkets.FirstOrDefault(c => c.Countries.Contains(countryCode)).GetConfiguration();
        }
        /// <summary>
        /// Gets the configuration for the market name provided.
        /// </summary>
        public static IMarketConfiguration GetMarketConfiguration(MarketName marketName)
        {
            return GlobalSettings.Markets.AvailableMarkets.Where(c => c.Name == marketName).FirstOrDefault().GetConfiguration();
        }

        /// <summary>
        /// Gets the configuration, based on the country cookie
        /// </summary>
        public static Market GetCurrentMarket()
        {
            var countryCode = GlobalUtilities.GetSelectedCountryCode();

            return GlobalSettings.Markets.AvailableMarkets.FirstOrDefault(c => c.Countries.Contains(countryCode));
        }
        /// <summary>
        /// Sets the CurrentUICulture of the site based on the user's language preferences.
        /// </summary>
        public static void SetCurrentUICulture(string cultureCode)
        {
            if (!HttpContext.Current.Request.IsAuthenticated) return;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(cultureCode);
        }
    }
}