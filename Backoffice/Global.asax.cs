using Common;
using Common.Helpers;
using System;
using System.Globalization;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.WebPages;

namespace Backoffice
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static DateTime ApplicationStartDate;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DisplayConfig.RegisterDisplayModes(DisplayModeProvider.Instance.Modes);
            ModelBinderConfig.RegisterModelBinders(ModelBinders.Binders);

            // Set the application's start date for easy reference
            ApplicationStartDate = DateTime.Now;
        }

        public override void Init()
        {
            this.PostAuthenticateRequest += new EventHandler(MvcApplication_PostAuthenticateRequest);
            base.Init();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            try
            {
                if (GlobalSettings.ErrorLogging.ErrorLoggingEnabled && !Request.IsLocal)
                {
                    ErrorLogger.LogException(Server.GetLastError(), Request.RawUrl);
                }
            }
            catch { }
        }

        void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var identity = UserIdentity.Deserialize(authCookie.Value);
                if (identity == null)
                {
                    FormsAuthentication.SignOut();
                }
                else
                {
                    var principal = new GenericPrincipal(identity, null);
                    HttpContext.Current.User = principal;
                    System.Threading.Thread.CurrentPrincipal = principal;
                    Context.User = new GenericPrincipal(identity, null);


                    // Set the culture
                    GlobalUtilities.SetCurrentCulture(Identity.Current.Market.CultureCode);

                    // Set the language
                    System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(GlobalUtilities.GetSelectedLanguage());
                }
            }
        }
    }
}
