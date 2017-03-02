using Backoffice.ViewModels;
using Common;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Dapper;
using Backoffice.Filters;

namespace Backoffice.Controllers
{
    [RoutePrefix("")]
    public class DashboardController : Controller
    {
        [Route("message")]
        public ActionResult LoginLanding()
        {
            return View();
        }

        [Route(""), BackofficeSubscriptionRequired]
        public ActionResult Index()
        {
            var model = new DashboardViewModel();
            var tasks = new List<Task>();
            var customerID = Identity.Current.CustomerID;

            if (!GlobalSettings.Globalization.HideForLive)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        model.RecentOrders = Exigo.GetCustomerOrders(new GetCustomerOrdersRequest
                        {
                            CustomerID = customerID,
                            IncludeOrderDetails = false,
                            Page = 1,
                            RowCount = 4
                        }).ToList();
                    }
                    catch (Exception ex) { }

                }));
            }
                // Get the volumes
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        model.Volumes = Exigo.GetCustomerVolumes(new GetCustomerVolumesRequest
                        {
                            CustomerID = customerID,
                            PeriodTypeID = PeriodTypes.Default,
                            VolumeIDs = new[] { 1, 2, 4, 16, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 91 }
                        });
                    }
                    catch (Exception ex) { }

                }));

                // Get the current commissions
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        model.CurrentCommissions = Exigo.GetCustomerRealTimeCommissions(new GetCustomerRealTimeCommissionsRequest
                        {
                            CustomerID = customerID
                        }).ToList();
                    }
                    catch (Exception) { }

                }));

                // Get the customer's recent organization activity
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        model.RecentActivities = Exigo.GetCustomerRecentActivity(new GetCustomerRecentActivityRequest
                        {
                            CustomerID = customerID,
                            Page = 1,
                            RowCount = 50
                        }).ToList();
                    }
                    catch (Exception ex) { }

                }));


                // Get the newest distributors
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        model.NewestDistributors = Exigo.GetNewestDistributors(new GetNewestDistributorsRequest
                        {
                            CustomerID = customerID,
                            RowCount = 12,
                            MaxLevel = 99999
                        }).Where(c => c.CustomerID != customerID).ToList();
                    }
                    catch (Exception ex) { }


                }));


                // Perform all tasks
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();

                return View(model);
            
        }

        public ActionResult GetRankAdvancementCard(int rankid)
        {
            var ranks = Exigo.GetRanks().ToList();

            GetCustomerRankQualificationsResponse model = null;

            // Check to ensure that the rank we are checking is not the last rank.
            // If so, return a null. Our view will take care of nulls specially.

          
            if (ranks.Last().RankID != rankid)
            {
                var Rank = ranks.Where(c => c.RankID > rankid).OrderBy(c => c.RankID).FirstOrDefault().RankID;
                model = Exigo.GetCustomerRankQualifications(new GetCustomerRankQualificationsRequest
                {
                    CustomerID = Identity.Current.CustomerID,
                    PeriodTypeID = PeriodTypes.Default,
                    RankID = Rank
                });
            }

            return PartialView("Cards/RankAdvancement", model);
        }
    }
}