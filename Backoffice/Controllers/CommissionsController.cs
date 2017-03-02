using Backoffice.ViewModels;
using Common;
using Common.Services;
using ExigoService;
using ExigoWeb.Kendo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Backoffice.Filters;

namespace Backoffice.Controllers
{
    [BackofficeSubscriptionRequired]
    public class CommissionsController : Controller
    {
        #region Commissions
        [Route("~/commissions/{runid:int=0}")]
        public ActionResult CommissionDetail(int runid)
        {
            var model = new CommissionDetailViewModel();

            // View Requests
            if (!Request.IsAjaxRequest())
            {
                model.CommissionPeriods = Exigo.GetCommissionPeriodList(Identity.Current.CustomerID);
                return View("CommissionDetail", model);
            }

            // AJAX requests
            else
            {
                // Real-time commissions
                if (runid == 0)
                {
                    model.Commissions = Exigo.GetCustomerRealTimeCommissions(new GetCustomerRealTimeCommissionsRequest
                    {
                        CustomerID = Identity.Current.CustomerID,
                        VolumeIDs = new[] { 1, 2, 3, 4, 16 }
                    });
                    return PartialView("_RealTimeCommissionDetail", model);
                }

                // Historical Commissions
                else
                {
                    model.Commissions = new List<ICommission>() { Exigo.GetCustomerHistoricalCommission(Identity.Current.CustomerID, runid) };
                    return PartialView("_HistoricalCommissionDetail", model);
                }
            }
        }

        [HttpPost]
        public JsonNetResult GetRealTimeBonusDetails(KendoGridRequest request)
        {
            var results = new List<RealTimeCommissionBonusDetail>();


            // Get the commission record(s)
            var context = Exigo.WebService();
            var realtimeresponse = context.GetRealTimeCommissions(new Common.Api.ExigoWebService.GetRealTimeCommissionsRequest
            {
                CustomerID = Identity.Current.CustomerID
            });
            if (realtimeresponse.Commissions.Length == 0) return new JsonNetResult();


            // Get the bonuses (I know, this is brutal, but our UI depends on it)
            foreach (var commission in realtimeresponse.Commissions)
            {
                var bonuses = commission.Bonuses
                    .Select(c => new CommissionBonus()
                    {
                        BonusID = c.BonusID,
                        BonusDescription = c.Description
                    }).Distinct();

                foreach (var bonusID in commission.Bonuses.Select(c => c.BonusID).Distinct())
                {
                    var bonus = bonuses.Where(c => c.BonusID == bonusID).FirstOrDefault();

                    // Get the details for this bonus
                    var details = context.GetRealTimeCommissionDetail(new Common.Api.ExigoWebService.GetRealTimeCommissionDetailRequest
                    {
                        CustomerID = commission.CustomerID,
                        PeriodType = commission.PeriodType,
                        PeriodID = commission.PeriodID,
                        BonusID = bonusID
                    }).CommissionDetails;


                    // Get the period details for this period
                    var period = Exigo.GetPeriods(new GetPeriodsRequest
                    {
                        PeriodTypeID = commission.PeriodType,
                        PeriodIDs = new int[] { commission.PeriodID }
                    }).FirstOrDefault();


                    // Format and save each bonus
                    foreach (var detail in details)
                    {
                        var typedDetail = (CommissionBonusDetail)detail;
                        var result = GlobalUtilities.Extend(typedDetail, new RealTimeCommissionBonusDetail());

                        result.BonusID = bonus.BonusID;
                        result.BonusDescription = bonus.BonusDescription;
                        result.PeriodDescription = period.PeriodDescription;
                        results.Add(result);
                    }
                }
            }


            // Filtering
            foreach (var filter in request.FilterObjectWrapper.FilterObjects)
            {
                results = results.AsQueryable().Where(filter.Field1, filter.Operator1, filter.Value1).ToList();
            }

            // Sorting
            foreach (var sort in request.SortObjects)
            {
                results = results.AsQueryable().OrderBy(sort.Field, sort.Direction).ToList();
            }


            // Return the data
            return new JsonNetResult(new
            {
                data = results
            });
        }

        [HttpPost]
        public JsonNetResult GetHistoricalBonusDetails(KendoGridRequest request, int runid)
        {
            // Recursively get the details
            dynamic data = new List<dynamic>();

            int lastResultCount = request.PageSize;
            int callsMade = 0;

            while (lastResultCount == request.PageSize)
            {
                // Establish the query
                var query = Exigo.OData().CommissionDetails
                    .Where(c => c.CustomerID == Identity.Current.CustomerID)
                    .Where(c => c.CommissionRunID == runid);


                // Filtering
                foreach (var filter in request.FilterObjectWrapper.FilterObjects)
                {
                    query = query.Where(filter.Field1, filter.Operator1, filter.Value1);
                }

                // Sorting
                foreach (var sort in request.SortObjects)
                {
                    query = query.OrderBy(sort.Field, sort.Direction);
                }

                // Fetch the data
                var results = query
                    .Select(c => new
                    {
                        c.BonusID,
                        Bonus_BonusDescription = c.Bonus.BonusDescription,
                        c.FromCustomerID,
                        FromCustomer_LastName = c.FromCustomer.FirstName + " " + c.FromCustomer.LastName,
                        c.Level,
                        c.PaidLevel,
                        c.SourceAmount,
                        c.Percentage,
                        c.CommissionAmount
                    })
                    .Skip(callsMade * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                data.AddRange(results);

                callsMade++;
                lastResultCount = results.Count;
            }


            // Return the data
            return new JsonNetResult(new
            {
                data = data
            });
        }

        #endregion

        #region Rank
        [Route("~/rank")]
        public ActionResult Rank()
        {
            var model = new RankViewModel();

            model.Ranks = Exigo.GetRanks().OrderBy(c => c.RankID);

            var currentperiod = Exigo.GetCurrentPeriod(PeriodTypes.Default);
            model.CurrentRank = Exigo.GetCustomerVolumes(new GetCustomerVolumesRequest
            {
                CustomerID = Identity.Current.CustomerID,
                PeriodTypeID = currentperiod.PeriodTypeID,
                PeriodID = currentperiod.PeriodID
            }).PayableAsRank;

            return View(model);
        }
        [HttpPost]
        public JsonNetResult GetRankQualifications(int rankID = 0)
        {
            var response = Exigo.GetCustomerRankQualifications(new GetCustomerRankQualificationsRequest
            {
                CustomerID = Identity.Current.CustomerID,
                PeriodTypeID = PeriodTypes.Default,
                RankID = rankID
            });

            var html = (!response.IsUnavailable) ? this.RenderPartialViewToString("_RankQualificationDetail", response) : "";

            return new JsonNetResult(new
            {
                result = response,
                html = html
            });
        }
        #endregion

        #region Volumes
        [Route("~/volumes")]
        public ActionResult VolumeList(KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return View();


            // Establish the query
            using (var context = new KendoGridDataContext(Exigo.Sql()))
            {
                return context.Query(request, @"
                     SELECT
                 pv.PeriodID,
				 p.StartDate,
				 p.EndDate,
				 p.PeriodDescription,
				 pv.PaidRankID,
				 r.RankDescription,
				 Team1TGBV = pv.Volume55,
				 Team2TGBV = pv.Volume56,
				 Team3TGBV = pv.Volume57,
				 Team4TGBV = pv.Volume58,
				 Team5TGBV = pv.Volume59,
                 Team6TGBV = pv.Volume91
                    FROM
	                    PeriodVolumes pv
						inner join Periods p
						on p.PeriodID = pv.PeriodID
						and p.PeriodTypeID = pv.PeriodTypeID
						left join Ranks r
						on r.RankID = pv.PaidRankID
                    WHERE
	                    p.StartDate <= GetDate() + 1
                        AND pv.PeriodTypeID = @periodtypeid
						and pv.CustomerID = @id
                ", new
            {
                id = Identity.Current.CustomerID,
                periodtypeid = PeriodTypes.Default
            }).Tokenize("CustomerID");
            }
        }
        #endregion
    }
}