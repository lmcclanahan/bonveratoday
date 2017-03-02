using System.Linq;
using System.Collections.Generic;
using ReplicatedSite.Models.SiteMap;
using Common;

namespace ReplicatedSite
{
    /// <summary>
    /// Site-specific settings
    /// </summary>
    public static partial class Settings
    {
        /// <summary>
        /// Site-wide navigation configurations
        /// </summary>
        public static class SiteMap
        {
            public static NavigationSiteMap Current
            {
                get
                {
                    return new NavigationSiteMap()
                    {
                        Items = new List<ISiteMapNode>()
                        {
                            new NavigationSiteMapNode("home", Resources.Common.Home, "home") { Action = "index", Controller = "home"},
                            new NavigationSiteMapNode("shop", Resources.Common.Shop, "shopping") { Action = "itemlist", Controller = "shopping", RouteParameters = new { parentCategoryKey = "", subCategoryKey = "" }, 
                                Children = new List<ISiteMapNode>()
                                {
                                    new NavigationSiteMapNode("ourproducts", Resources.Common.Products, "ourproducts") { Action = "ourproducts", Controller = "shopping" },
                                    new NavigationSiteMapNode("partnerstores", Resources.Common.PartnerStores, "partnerstores") { Action = "partnerstores", Controller = "shopping" },
                                    new NavigationSiteMapNode("affiliatestores", Resources.Common.AffiliateStores, "affiliatestores") { Action = "affiliatestores", Controller = "shopping" },
                                    new NavigationSiteMapNode("cart", Resources.Common.MyCart, "cart") { Action = "cart", Controller = "shopping" }
                                } 
                            },
                            new NavigationSiteMapNode("replenishments", Resources.Common.AutoOrders, "replenishments") { Action = "autoorderpreferences", Controller = "autoorders", IsVisible = () => (Identity.Customer != null)},
                            new NavigationSiteMapNode("about", Resources.Common.About, "about", new List<ISiteMapNode>()
                            {
                                new NavigationSiteMapNode("companyoverview", Resources.Common.CompanyOverview) { Action = "companyoverview", Controller = "home" },
                                new NavigationSiteMapNode("ourmission", Resources.Common.OurMission) { Action = "ourmission", Controller = "home" },
                                new NavigationSiteMapNode("compensation", Resources.Common.CompensationPlanOverview) { Url="http://bonvera.com/Content/documents/Bonvera_Overview_Revised_Final_07_05_16.pdf", Target="_blank" },
                                new NavigationSiteMapNode("faq", Resources.Common.FAQ) { Action = "faq", Controller = "home" }
                            }),                                                        
                            new NavigationSiteMapNode("login", Resources.Common.SignIn, "login") { Action = "login", Controller = "account", IsVisible = () => (Identity.Customer == null)},
                            new NavigationSiteMapNode("account", Resources.Common.Account, "account", new List<ISiteMapNode>()
                            {
                                new NavigationSiteMapNode("accountsettings", Resources.Common.AccountSettings) { Action = "index", Controller = "account" },
                                new NavigationSiteMapNode("orders", Resources.Common.Orders) { Action = "orderlist", Controller = "account" },
                                new NavigationSiteMapNode("addresses", Resources.Common.Addresses) { Action = "addresslist", Controller = "account" },
                                new NavigationSiteMapNode("paymentmethods", Resources.Common.PaymentMethods) { Action = "paymentmethodlist", Controller = "account" },
                                new NavigationSiteMapNode("autoordermanager", Resources.Common.AutoOrders) { Action = "autoorderpreferences", Controller = "autoorders" },
                                new NavigationSiteMapNode("signout", Resources.Common.SignOut) { Action = "logout", Controller = "account" }
                            }),                            
                            new NavigationSiteMapNode("signout", Resources.Common.SignOut) { Action = "logout", Controller = "account", DeviceVisibilityCssClass = "visible-xs" }
                        }
                    };
                }
            }
        }
    }
}