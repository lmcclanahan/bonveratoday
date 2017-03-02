using System.Collections.Generic;
using ExigoService;
using OData = Common.Api.ExigoOData;

namespace Backoffice.ViewModels
{
    public class MarketListViewModel
    {
        public MarketListViewModel()
        {
            Markets = new List<Market>();
            Warehouses = new List<OData.Warehouse>();
            Countries = new List<OData.Country>();
            PriceTypes = new List<OData.PriceType>();
            Languages = new List<OData.Language>();
            ShipMethods = new List<OData.ShipMethod>();
            WebCategories = new List<OData.WebCategory>();
        }


        public List<Market> Markets { get; set; }
        public List<OData.Warehouse> Warehouses { get; set; }
        public List<OData.Country> Countries { get; set; }
        public List<OData.PriceType> PriceTypes { get; set; }
        public List<OData.Language> Languages { get; set; }
        public List<OData.ShipMethod> ShipMethods { get; set; }
        public List<OData.WebCategory> WebCategories { get; set; }
    }
}