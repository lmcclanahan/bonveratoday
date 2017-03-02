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
    public class BackofficeSubscriptionRequiredAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Validate the login
            var identity = httpContext.User.Identity as UserIdentity;
            if (identity == null) return true;

            // Do they have a valid subscription?
            return identity.IsSubscribedMonthly;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                { "controller", "subscriptions" },
                { "action", "index" }
            });
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