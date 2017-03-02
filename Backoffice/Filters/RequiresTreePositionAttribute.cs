using ExigoService;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Backoffice.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class RequiresTreePositionAttribute : AuthorizeAttribute
    {
        public TreeType Tree { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            switch (Tree)
            {
                case TreeType.UniLevel:
                    return Exigo.IsCustomerInUniLevelTree(Identity.Current.CustomerID);
                case TreeType.Binary:
                    return Exigo.IsCustomerInBinaryTree(Identity.Current.CustomerID);
            }

            return true;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary 
            {
                { "controller", "error" },
                { "action", "treeplacementrequired" }
            });
        }
    }

    public enum TreeType
    {
        UniLevel,
        Binary
    }
}