using Backoffice.ViewModels;
using Common;
using Common.Services;
using ExigoService;
using ExigoWeb.Kendo;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Backoffice.Controllers
{
    [RoutePrefix("profile")]
    [Route("{action=index}")]
    public class ProfileController : Controller
    {        
        public ActionResult Index(string token)
        {
            var model = new ProfileViewModel();
            var id = Convert.ToInt32(Security.Decrypt(token, Identity.Current.CustomerID));           

            if (id == 0) id = Identity.Current.CustomerID;

            model.Customer = Exigo.GetCustomer(id);
            model.RecentActivity = Exigo.GetCustomerRecentActivity(new GetCustomerRecentActivityRequest
            {
                CustomerID = id
            });
            model.Volumes = Exigo.GetCustomerVolumes(new GetCustomerVolumesRequest
            {
                CustomerID   = id,
                PeriodTypeID = PeriodTypes.Default                
            });
        


            if (Request.IsAjaxRequest())
            {
                try
                {
                    var customerTypeHtml = model.Customer.CustomerType.CustomerTypeDescription + " Details";
                    var html = this.RenderPartialViewToString("Partials/_Profile", model);
                    return new JsonNetResult(new
                {
                    success = true,
                    html = html,
                    customertypehtml = customerTypeHtml
                });

                }
                catch(Exception e)
                {
                         return new JsonNetResult(new
                {
                    success = false,
                    message = e.Message
                });
                }

            }
            else return View(model);
        }

        [Route("popoversummary/{id:int=0}")]
        public ActionResult PopoverSummary(int id)
        {
            var model = new ProfileViewModel();

            if (id == 0) id = Identity.Current.CustomerID;

            model.Customer = Exigo.GetCustomer(id);

            if (Request.IsAjaxRequest()) return PartialView("Partials/_ProfilePopover", model);
            else return View(model);
        }

        [Route("volumes/{id:int}")]
        public ActionResult VolumesList(int id, KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return PartialView("Partials/VolumesList");

            //TODO Convert to SQL

            // Get the customer's start date
            var customerStartDate = Exigo.OData().Customers
                .Where(c => c.CustomerID == id)
                .Select(c => c.CreatedDate)
                .Single();


            // Establish the query
            var query = Exigo.OData().PeriodVolumes
                .Where(c => c.CustomerID == id)
                .Where(c => c.PeriodTypeID == PeriodTypes.Default)
                .Where(c => c.Period.StartDate <= DateTime.Now)
                .Where(c => c.Period.EndDate > customerStartDate);


            // Fetch the data
            var context = new KendoGridDataContext();
            return context.Query(request, query, c => new
            {
                PeriodID = c.PeriodID,                
                Period_EndDate = c.Period.EndDate,                                
                PaidRank_RankDescription = c.PaidRank.RankDescription,
                Volume55 = c.Volume55,
                Volume56 = c.Volume56,
                Volume57 = c.Volume57,
                Volume58 = c.Volume58,
                Volume59 = c.Volume59
            });
        }

        [Route("orders/{id:int}")]
        public ActionResult OrdersList(int id, KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return PartialView("Partials/OrdersList");

            //TODO Convert to SQL

            // Establish the query
            var query = Exigo.OData().Orders
                .Where(c => c.CustomerID == id)
                .Where(c => c.OrderStatusID >= OrderStatuses.Accepted);


            var context = new KendoGridDataContext();
            return context.Query(request, query, c => new
            {
                OrderID = c.OrderID,
                OrderDate = c.OrderDate,
                SubTotal = c.SubTotal,
                // the naming here is misleading BV is actually stored in the CV table, but has been left as is for sorting purposes
                CommissionableVolumeTotal = c.CommissionableVolumeTotal
            });
        }

        [Route("autoorders/{id:int}")]
        public ActionResult AutoOrdersList(int id, KendoGridRequest request = null)
        {
            if (Request.HttpMethod.ToUpper() == "GET") return PartialView("Partials/AutoOrdersList");

            //TODO Convert to SQL

            // Establish the query
            var query = Exigo.OData().AutoOrders
                .Where(c => c.CustomerID == id)
                .Where(c => c.AutoOrderStatusID == 0);

            var context = new KendoGridDataContext();
            return context.Query(request, query, c => new
            {
                AutoOrderID = c.AutoOrderID,
                LastRunDate = c.LastRunDate,
                NextRunDate = c.NextRunDate,
                SubTotal = c.SubTotal,
                // the naming here is misleading BV is actually stored in the CV table, but has been left as is for sorting purposes
                CommissionableVolumeTotal = c.CommissionableVolumeTotal
            });
        }
        
    }
}