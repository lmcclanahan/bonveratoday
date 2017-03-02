using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReplicatedSite.ViewModels
{
    // Used for Autoship Pop up modal
    public class AutoOrderChoiceViewModel
    {
        public AutoOrderChoiceViewModel()
        {
            this.AutoOrders = new List<AutoOrder>();
            this.AutoOrderItems = new List<Item>();
        }

        public List<AutoOrder> AutoOrders { get; set; }
        public List<Item> AutoOrderItems { get; set; }
        public OrderCalculationResponse CalculatedAutoOrder { get; set; }
    }
}