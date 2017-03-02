using System.Linq;
using System.Collections.Generic;
using Backoffice.Models.SiteMap;
using Common;

namespace Backoffice
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
                    var Items = new List<ISiteMapNode>();

                    // SiteMap without items hidden
                    if (!GlobalSettings.HideForLive)
                    {
                        Items = new List<ISiteMapNode>()
                        {
                            new NavigationSiteMapNode("dashboard") { Icon = "fa-home", Action = "index", Controller = "dashboard", ShowWhenNotSubscribedMonthly = false  },

                            new NavigationSiteMapNode("commissions", Resources.Common.Commissions, new List<ISiteMapNode>()                    
                            {
                                new NavigationSiteMapNode("page-commissions", Resources.Common.Commissions) { Action = "commissiondetail", Controller = "commissions", ShowWhenNotSubscribedMonthly = false },
                                new NavigationSiteMapNode("page-rank", Resources.Common.RankAdvancement) { Action = "rank", Controller = "commissions", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("page-volumes", Resources.Common.Volumes) { Action = "volumelist", Controller = "commissions", ShowWhenNotSubscribedMonthly = false  }
                            }),
                            
                            new NavigationSiteMapNode("organization", Resources.Common.Organization, new List<ISiteMapNode>()
                            {
                                new NavigationSiteMapNode("enroll", Resources.Common.EnrollNew + " Associate") { Action="EnrollmentRedirect", Controller="App", Icon = "fa-plus", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("enroll", "Register Retail Customer") { Action="RetailEnrollmentRedirect", Controller="App", Icon = "fa-plus", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("enroll", "Register Smart Shopper") { Action="SmartShopperEnrollmentRedirect", Controller="App", Icon = "fa-plus", ShowWhenNotSubscribedMonthly = false  },
                                new DividerNode(),
                                new NavigationSiteMapNode("myteam", Resources.Common.MyTeam) { Action = "MyTeam", Controller = "organization", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("personallyenrolled", Resources.Common.PersonallyEnrolledTeam) { Action = "personallyenrolledlist", Controller = "organization", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("upcomingpromotions", Resources.Common.UpcomingPromotions) { Action = "upcomingpromotionslist", Controller = "organization", ShowWhenNotSubscribedMonthly = false  },                        
                                new NavigationSiteMapNode("downlineranks", Resources.Common.DownlineRanks) { Action = "downlinerankslist", Controller = "organization", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("downlineorders", Resources.Common.DownlineOrders) { Action = "downlineorderslist", Controller = "organization",  ShowWhenNotSubscribedMonthly = false, IsVisible = () => (Identity.Current.Ranks.HighestPaidRankInAnyPeriod.RankID >= Ranks.SeniorDirector) },
                                new NavigationSiteMapNode("downlineautoorders", Resources.Common.DownlineAutoOrders) { Action = "downlineautoorderslist", Controller = "organization", ShowWhenNotSubscribedMonthly = false , IsVisible = () => (Identity.Current.Ranks.HighestPaidRankInAnyPeriod.RankID >= Ranks.SeniorDirector) },
                                

                                new NavigationSiteMapNode("newdistributors", Resources.Common.NewDistributorsList) { Action = "NewDistributorsList", Controller = "organization" , ShowWhenNotSubscribedMonthly = false },
                                new NavigationSiteMapNode("preferredcustomers", Resources.Common.PreferredCustomers) { Action = "preferredcustomerlist", Controller = "organization", ShowWhenNotSubscribedMonthly = false  }                                
                            }),

                            new NavigationSiteMapNode("autoorders", Resources.Common.AutoOrders) { Action = "AutoOrderPreferences", Controller = "autoorders" },
                            
                            new NavigationSiteMapNode("orders", Resources.Common.Orders) { Action = "orderlist", Controller = "orders" },

                            //Z.M. 76882 5-10-16 Added corporate blog link-
                            new NavigationSiteMapNode("resources", Resources.Common.Resources, new List<ISiteMapNode>()
                            {
                                // Use this one when resource library is needed fully functional instead of hard coded page - Mike M.
                                //new NavigationSiteMapNode("resourcelist", Resources.Common.ResourcesLibrary) { Action = "resourcelist", Controller = "resources", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("resourcelist", Resources.Common.ResourcesLibrary) { Action = "index", Controller = "resources", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("manageresources", Resources.Common.ManageResources) { Action = "manageresources", Controller = "resources", IsVisible = () => new[] { 1 }.Contains(Identity.Current.CustomerID), ShowWhenNotSubscribedMonthly = false },
                                new NavigationSiteMapNode("corporateblog", Resources.Common.CorporateBlog) { Action = "corporateblog", Controller = "app", Target = "_blank"},
                                // P.M. 78700 7/21/2016 Added orderbusinesscard Map Node
                                new NavigationSiteMapNode("orderbusinesscards", Resources.Common.OrderBusinessCards){ Action = "orderbusinesscards", Controller = "app", Target = "_blank"}
                            }),

                            new NavigationSiteMapNode("contactus", Resources.Common.ContactUs) { Action = "", Controller = "" },

                            new NavigationSiteMapNode("store", Resources.Common.Shop)
                            {
                                Action = "itemlist", Controller = "shopping", Children = new List<ISiteMapNode>()
                                {
                                    new NavigationSiteMapNode("items", Resources.Common.Products) { Action = "itemlist", Controller = "shopping" },
                                    new NavigationSiteMapNode("partnerstores", Resources.Common.PartnerStores) { Action = "partnerstores", Controller = "shopping" },
                                    new NavigationSiteMapNode("affiliatestores", Resources.Common.AffiliateStores) { Action = "affiliatestores", Controller = "shopping" },
                                    new NavigationSiteMapNode("cart", Resources.Common.MyCart) { Action = "cart", Controller = "shopping" }
                                }
                            },

                            new NavigationSiteMapNode("account", Resources.Common.Account, new List<ISiteMapNode>()
                            {
                                new NavigationSiteMapNode("accountsettings", Resources.Common.AccountSettings) { Action = "index", Controller = "account" },
                                new NavigationSiteMapNode("avatar", Resources.Common.Avatar) { Action = "manageavatar", Controller = "account" },
                                new NavigationSiteMapNode("addresses", Resources.Common.Addresses) { Action = "addresslist", Controller = "account" },
                                new NavigationSiteMapNode("paymentmethods", Resources.Common.PaymentMethods) { Action = "paymentmethodlist", Controller = "account" },
                                new NavigationSiteMapNode("directdeposit", Resources.Common.DirectDeposit) { Action = "commissionpayout", Controller = "account" },
                                new NavigationSiteMapNode("notifications", Resources.Common.Notifications) { Action = "notifications", Controller = "account" },
                            }) { DeviceVisibilityCssClass = "visible-xs" },

                            new NavigationSiteMapNode("signout", Resources.Common.SignOut) { Action = "logout", Controller = "authentication", DeviceVisibilityCssClass = "visible-xs" }
                        };

                    }
                    else
                    {
                        // SiteMap with items hidden
                        Items = new List<ISiteMapNode>()
                        {
                            new NavigationSiteMapNode("dashboard") { Icon = "fa-home", Action = "index", Controller = "dashboard", ShowWhenNotSubscribedMonthly = false  },

                            new NavigationSiteMapNode("commissions", Resources.Common.Commissions, new List<ISiteMapNode>()                    
                            {
                                new NavigationSiteMapNode("page-commissions", Resources.Common.Commissions) { Action = "commissiondetail", Controller = "commissions", ShowWhenNotSubscribedMonthly = false },
                                new NavigationSiteMapNode("page-rank", Resources.Common.RankAdvancement) { Action = "rank", Controller = "commissions", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("page-volumes", Resources.Common.Volumes) { Action = "volumelist", Controller = "commissions", ShowWhenNotSubscribedMonthly = false  }
                            }),
                            
                            new NavigationSiteMapNode("organization", Resources.Common.Organization, new List<ISiteMapNode>()
                            {
                                new NavigationSiteMapNode("enroll", Resources.Common.EnrollNew) { Action="EnrollmentRedirect", Controller="App", Icon = "fa-plus", ShowWhenNotSubscribedMonthly = false  },
                                new DividerNode(),
                                new NavigationSiteMapNode("myteam", Resources.Common.MyTeam) { Action = "MyTeam", Controller = "organization", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("personallyenrolled", Resources.Common.PersonallyEnrolledTeam) { Action = "personallyenrolledlist", Controller = "organization", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("upcomingpromotions", Resources.Common.UpcomingPromotions) { Action = "upcomingpromotionslist", Controller = "organization", ShowWhenNotSubscribedMonthly = false  },                        
                                new NavigationSiteMapNode("downlineranks", Resources.Common.DownlineRanks) { Action = "downlinerankslist", Controller = "organization", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("downlineorders", Resources.Common.DownlineOrders) { Action = "downlineorderslist", Controller = "organization",  ShowWhenNotSubscribedMonthly = false, IsVisible = () => (Identity.Current.Ranks.HighestPaidRankInAnyPeriod.RankID > Ranks.Professional) },
                                new NavigationSiteMapNode("downlineautoorders", Resources.Common.DownlineAutoOrders) { Action = "downlineautoorderslist", Controller = "organization", ShowWhenNotSubscribedMonthly = false , IsVisible = () => (Identity.Current.Ranks.HighestPaidRankInAnyPeriod.RankID > Ranks.Professional) },
                                

                                new NavigationSiteMapNode("newdistributors", Resources.Common.NewDistributorsList) { Action = "NewDistributorsList", Controller = "organization" , ShowWhenNotSubscribedMonthly = false },
                                new NavigationSiteMapNode("preferredcustomers", Resources.Common.PreferredCustomers) { Action = "preferredcustomerlist", Controller = "organization", ShowWhenNotSubscribedMonthly = false  }                                
                            }),

                    
                    
                            new NavigationSiteMapNode("resources", Resources.Common.Resources, new List<ISiteMapNode>()
                            {
                                // Use this one when resource library is needed fully functional instead of hard coded page - Mike M.
                                //new NavigationSiteMapNode("resourcelist", Resources.Common.ResourcesLibrary) { Action = "resourcelist", Controller = "resources", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("resourcelist", Resources.Common.ResourcesLibrary) { Action = "index", Controller = "resources", ShowWhenNotSubscribedMonthly = false  },
                                new NavigationSiteMapNode("manageresources", Resources.Common.ManageResources) { Action = "manageresources", Controller = "resources", IsVisible = () => new[] { 1 }.Contains(Identity.Current.CustomerID), ShowWhenNotSubscribedMonthly = false },
                                new NavigationSiteMapNode("corporateblog", Resources.Common.CorporateBlog) { Action = "corporateblog", Controller = "app", Target = "_blank"},
                                // P.M. 78700 7/21/2016 Added orderbusinesscard Map Node
                                new NavigationSiteMapNode("orderbusinesscards", Resources.Common.OrderBusinessCards){ Action = "orderbusinesscards", Controller = "app", Target = "_blank"}
                            }),

                             new NavigationSiteMapNode("store", Resources.Common.Shop)
                            {
                                Action = "itemlist", Controller = "shopping", Children = new List<ISiteMapNode>()
                                {
                                    new NavigationSiteMapNode("items", Resources.Common.Products) { Action = "itemlist", Controller = "shopping" },
                                    new NavigationSiteMapNode("partnerstores", Resources.Common.PartnerStores) { Action = "partnerstores", Controller = "shopping" }
                                }
                            },

                            new NavigationSiteMapNode("account", Resources.Common.Account, new List<ISiteMapNode>()
                            {
                                new NavigationSiteMapNode("accountsettings", Resources.Common.AccountSettings) { Action = "index", Controller = "account" },
                                new NavigationSiteMapNode("avatar", Resources.Common.Avatar) { Action = "manageavatar", Controller = "account" },
                                new NavigationSiteMapNode("addresses", Resources.Common.Addresses) { Action = "addresslist", Controller = "account" },
                                new NavigationSiteMapNode("paymentmethods", Resources.Common.PaymentMethods) { Action = "paymentmethodlist", Controller = "account" },
                                new NavigationSiteMapNode("directdeposit", Resources.Common.DirectDeposit) { Action = "commissionpayout", Controller = "account" },
                                new NavigationSiteMapNode("notifications", Resources.Common.Notifications) { Action = "notifications", Controller = "account" },
                            }) { DeviceVisibilityCssClass = "visible-xs" },

                            new NavigationSiteMapNode("signout", Resources.Common.SignOut) { Action = "logout", Controller = "authentication", DeviceVisibilityCssClass = "visible-xs" }
                        };
                    }

                    return new NavigationSiteMap { Items = Items };

                }
            }
        }
    }
}
