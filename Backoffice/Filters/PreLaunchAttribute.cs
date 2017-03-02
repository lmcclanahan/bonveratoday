using Backoffice.Models;
using Common;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Backoffice.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class PreLaunchHideAttribute : AuthorizeAttribute
    {
        public bool AllowPreLaunch = true;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            if (AllowPreLaunch == false && GlobalSettings.Globalization.HideForLive)
                    {
                        return FailAndRedirect(new RouteValueDictionary {
                            { "controller", "dashboard" },
                            { "action", "index" }});
                    }
                      
                  
            

          
            return true;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (RedirectRouteValues != null)
            {
                filterContext.Result = new RedirectToRouteResult(RedirectRouteValues);
            }
        }

        protected RouteValueDictionary RedirectRouteValues { get; set; }
        protected bool FailAndRedirect(RouteValueDictionary routeValues = null)
        {
            routeValues = routeValues ?? new RouteValueDictionary {
                { "controller", "error" },
                { "action", "notfound" }
            };

            RedirectRouteValues = routeValues;
            return false;
        }
    }
}


// OLD LOGIC - 2/3/2016
//using System;
//using System.Web.Mvc;

//namespace Backoffice.Filters
//{
//    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
//    public class BackofficeAuthorizeAttribute : AuthorizeAttribute
//    {
//         public override void OnAuthorization(AuthorizationContext filterContext)
//         {
//             if (filterContext.HttpContext.Request.IsAuthenticated)
//             {
//                 var identity = filterContext.HttpContext.User.Identity as UserIdentity;
//                 if (identity == null)
//                 {
//                     base.OnAuthorization(filterContext);
//                     return;
//                 }
//             }
//             else
//             {
//                 base.OnAuthorization(filterContext);
//             }
//         }
//    }