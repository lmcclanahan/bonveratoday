using Common;
using Common.Helpers;
using ReplicatedSite.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.WebPages;

namespace ReplicatedSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static DateTime ApplicationStartDate;

        public override void Init()
        {
            this.BeginRequest += new EventHandler(Application_BeginRequest);
            this.PostAuthenticateRequest += new EventHandler(MvcApplication_PostAuthenticateRequest);

            base.Init();
        }

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

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Get the route data
            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current));
            var defaultWebAlias = GlobalSettings.ReplicatedSites.DefaultWebAlias;
            var identityService = new IdentityService();

            // Account for attribute routing and null routeData
            if (routeData != null && routeData.Values.ContainsKey("MS_DirectRouteMatches"))
            {
                routeData = ((List<RouteData>)routeData.Values["MS_DirectRouteMatches"]).First();
            }

            // Added logic specific to soft launch, where only IA enrollment is allowed in the replicated and no web alias should be seen in the URL
            if (Common.GlobalSettings.Globalization.HideForLive)
            {
                HttpContext.Current.Items["OwnerWebIdentity"] = identityService.GetIdentity(defaultWebAlias);
                return;
            }


            // If we have an identity and the current identity matches the web alias in the routes, stop here.
            var identity = HttpContext.Current.Items["OwnerWebIdentity"] as ReplicatedSiteIdentity;
            if (routeData == null
                || routeData.Values["webalias"] == null
                || (identity != null && identity.WebAlias.Equals(routeData.Values["webalias"].ToString(), StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }


            // Determine some web alias data
            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current))));
            var currentWebAlias = routeData.Values["webalias"].ToString();
            var lastWebAlias = GlobalUtilities.GetLastWebAlias(defaultWebAlias);
            var defaultPage = urlHelper.Action(routeData.Values["action"].ToString(), routeData.Values["controller"].ToString(), new { webalias = lastWebAlias });


            // If we are an orphan and we don't allow them, redirect to a capture page.
            if (!Settings.AllowOrphans && currentWebAlias.Equals(defaultWebAlias, StringComparison.InvariantCultureIgnoreCase))
            {
                HttpContext.Current.Response.Redirect(urlHelper.Action("webaliasrequired", "error"));
            }


            // If we are an orphan, try to redirect the user back to a previously-visited replicated site
            if (Settings.RememberLastWebAliasVisited
                && currentWebAlias.Equals(defaultWebAlias, StringComparison.InvariantCultureIgnoreCase)
                && !defaultWebAlias.Equals(lastWebAlias, StringComparison.InvariantCultureIgnoreCase))
            {
                HttpContext.Current.Response.Redirect(defaultPage);
            }


            // Attempt to authenticate the web alias
            HttpContext.Current.Items["OwnerWebIdentity"] = identityService.GetIdentity(currentWebAlias);
            if (HttpContext.Current.Items["OwnerWebIdentity"] != null)
            {
                if (Settings.RememberLastWebAliasVisited && currentWebAlias.ToLower() != GlobalSettings.ReplicatedSites.DefaultWebAlias.ToLower())
                {
                    GlobalUtilities.SetLastWebAlias(currentWebAlias);
                }
                else
                {
                    GlobalUtilities.DeleteLastWebAlias();
                }
            }
            else
            {
                if (Settings.RememberLastWebAliasVisited)
                {
                    GlobalUtilities.DeleteLastWebAlias();
                    lastWebAlias = defaultWebAlias;
                    HttpContext.Current.Response.Redirect(defaultPage);
                }
                else
                {
                    HttpContext.Current.Response.Redirect(urlHelper.Action("invalidwebalias", "error"));
                }
            }
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
            var authenticated = false;

            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

            // Set the culture
            if (authCookie != null)
            {
                var identity = CustomerIdentity.Deserialize(authCookie.Value);
                if (identity == null)
                {
                    FormsAuthentication.SignOut();
                }
                else
                {
                    authenticated = true;

                    HttpContext.Current.User = new GenericPrincipal(identity, null);
                    Context.User = new GenericPrincipal(identity, null);


                    // Set the culture codes
                    GlobalUtilities.SetCurrentCulture(Identity.Customer.Market.CultureCode);
                }
            }
            else
            {
                var cultureCookie = HttpContext.Current.Request.Cookies[GlobalSettings.Globalization.SiteCultureCookieName];
                if (cultureCookie != null && cultureCookie.Value.IsNotNullOrEmpty())
                {
                    GlobalUtilities.SetCurrentCulture(cultureCookie.Value);
                }
            }

            // Set the language
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(GlobalUtilities.GetSelectedLanguage());
        }
    }
}