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
    public class BackofficeAuthorizeAttribute : AuthorizeAttribute
    {
        public bool RequiresLogin = true;
        public bool ValidateSubscription = true;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Validate the login
            UserIdentity identity = null;
            if (RequiresLogin)
            {
                identity = httpContext.User.Identity as UserIdentity;
                if (identity == null) return FailAndRedirect(new RouteValueDictionary {
                    { "controller", "authentication" },
                    { "action", "login" }
                });
            }

            //// Do they have a valid subscription?
            //if (ValidateSubscription)
            //{
            //    var subscription = Exigo.GetCustomerSubscription(identity.CustomerID, Subscriptions.BackofficeFeatures);
            //
            //    var isSubscribed = false;
            //    if (subscription == null || subscription.IsExpired)
            //    {
            //        var cookie = httpContext.Request.Cookies[GlobalSettings.Backoffices.MonthlySubscriptionCookieName];
            //        if (cookie == null)
            //        {
            //            if (subscription == null)
            //            {
            //                var newsub = Exigo.WebService().GetSubscription(new Common.Api.ExigoWebService.GetSubscriptionRequest
            //                {
            //                    CustomerID = Identity.Current.CustomerID,
            //                    SubscriptionID = Subscriptions.BackofficeFeatures
            //                });
            //                if (newsub != null && newsub.Status == Common.Api.ExigoWebService.SubscriptionStatus.Active)
            //                {
            //                    isSubscribed = true;
            //                }
            //                else
            //                {
            //                    var customer = Exigo.GetCustomer(Identity.Current.CustomerID);
            //
            //                    if (customer.CreatedDate >= DateTimeExtensions.ToCST(DateTime.Now.AddMinutes(-30)) && customer.CustomerTypeID == CustomerTypes.Associate)
            //                    {
            //
            //                        isSubscribed = true;
            //                        cookie = new HttpCookie(GlobalSettings.Backoffices.MonthlySubscriptionCookieName);
            //                        cookie.Value = "true";
            //                        cookie.Expires = DateTime.Now.AddMinutes(15);
            //                        httpContext.Response.Cookies.Add(cookie);
            //
            //                    }
            //                    else
            //                    {
            //
            //                    }
            //                }
            //            }
            //            //T.W. 77187 on 5/19/2016 removing condition as per Venkat, now all expired subscriptions will be taken to the shopping products view, EVEN the first time they revisit //their back office.
            //            if (!isSubscribed)
            //            {
            //                //// If they are redirecting to the dashboard, we need to send the user to the shopping cart page if they have already been a customer for more than 30 days - ticket /# /76366
            //                //var isDashboardRequest = httpContext.Request.RequestContext.RouteData.Values.Contains(new KeyValuePair<string, object>("controller", "Dashboard"));
            //                //var isInitialLoad = isDashboardRequest && httpContext.Request.UrlReferrer != null && httpContext.Request.UrlReferrer.AbsoluteUri.Contains("login");
            //
            //                //if (isInitialLoad)
            //                //{
            //                //    var createdDate = Exigo.OData().Customers.Where(c => c.CustomerID == identity.CustomerID).FirstOrDefault().CreatedDate;
            //
            //                //    if (createdDate.AddDays(30) <= DateTime.Now)
            //                //{
            //                return FailAndRedirect(new RouteValueDictionary {
            //                        { "controller", "shopping" },
            //                        { "action", "itemlist" }});
            //                //    }
            //                //}
            //
            //                //return FailAndRedirect(new RouteValueDictionary {
            //                //{ "controller", "subscriptions" },
            //                //{ "action", "index" }});
            //            }
            //        }
            //        else
            //        {
            //            Identity.Current.IsSubscribedMonthly = true;
            //        }
            //
            //    }
            //    else
            //    {
            //        Identity.Current.IsSubscribedMonthly = true;
            //    }
            //}

            // They passed the tests, they're good to go!
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