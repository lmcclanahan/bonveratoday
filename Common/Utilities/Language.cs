using Common.Api.ExigoWebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Linq;
using System.Globalization;

namespace Common
{
    public static partial class GlobalUtilities
    {
        public static int GetSelectedExigoLanguageID()
        {
            //If the cookie LanguagePreference exists, we set the culture code appropriately
            var languageKey = GlobalUtilities.GetSelectedLanguage();

            switch (languageKey)
            {
                // not being used
                case "fr-CA": return Languages.French;
                case "es-US": return Languages.Spanish;
                case "en-US":
                default: return Languages.English;
            }

        }        

        public static string GetCultureCode(int langID)
        {
            switch (langID)
            {
                case Languages.English:
                    return "en-US";
                case Languages.Spanish:
                    return "es-MX";                
                default:
                    return "en-US";
            }
        }

        public static string GetSelectedLanguage()
        {
            var languageCookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.LanguageCookieName];
            if (languageCookie != null)
            {
                try
                {
                    var culture = CultureInfo.CreateSpecificCulture(languageCookie.Value);
                    return culture.Name;
                }
                catch (Exception ex)
                {
                    languageCookie.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Current.Response.Cookies.Add(languageCookie);

                    var nativeCode = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                    switch (nativeCode)
                    {
                        case "en-US":
                            return "en-US";
                        default:
                            return "es-US";
                    }
                }
            }
            else
            {
                var nativeCode = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                switch (nativeCode)
                {
                    case "en-US":
                        return "en-US";
                    default:
                        return "es-US";
                }
            }
        }
    }
}