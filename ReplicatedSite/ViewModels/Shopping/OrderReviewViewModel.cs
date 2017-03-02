using ReplicatedSite.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class OrderReviewViewModel : IShoppingViewModel
    {
        public IEnumerable<IItem> OrderItems { get; set; }
        public IEnumerable<IItem> AutoOrderItems { get; set; }

        public OrderCalculationResponse OrderTotals { get; set; }
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public int ShipMethodID { get; set; }

        public IEnumerable<Common.Api.ExigoWebService.FrequencyType> AutoOrderFrequencyTypes { get; set; }
        public OrderCalculationResponse AutoOrderTotals { get; set; }
        public IEnumerable<IShipMethod> AutoOrderShipMethods { get; set; }
        public int AutoOrderShipMethodID { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }

        public int WillCallShipMethodID { get; set; }
       
        public decimal AvailablePoints { get; set; }
    }
}