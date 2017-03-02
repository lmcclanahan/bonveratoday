using Backoffice.ViewModels;
using Common;
using ExigoService;
using System.Web.Mvc;
using System.Linq;

namespace Backoffice.Controllers
{
    public class DevController : Controller
    {
        [Route("~/dev")]
        public ActionResult Design()
        {
            return View();
        }

        [Route("~/portals")]
        public ActionResult Portal()
        {
            return View();
        }

        [Route("~/dev/markets")]        
        public ActionResult MarketList()
        {
            var model = new MarketListViewModel();
            var context = Exigo.OData();
            model.Markets = GlobalSettings.Markets.AvailableMarkets;

            model.Warehouses = context.SelectAll((ctx) =>
                ctx.Warehouses);
            model.Countries = context.SelectAll((ctx) =>
                ctx.Countries);
            model.PriceTypes = context.SelectAll((ctx) =>
                ctx.PriceTypes);
            model.Languages = context.SelectAll((ctx) =>
                ctx.Languages);
            model.ShipMethods = context.SelectAll((ctx) =>
                ctx.ShipMethods);
            model.WebCategories = context.SelectAll((ctx) =>
                ctx.WebCategories.Where(c => c.WebID == 1));

            return View(model);
        }
	}
}