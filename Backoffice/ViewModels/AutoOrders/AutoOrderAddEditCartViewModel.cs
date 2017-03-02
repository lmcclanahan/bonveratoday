using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels.AutoOrders
{
    public class AutoOrderAddEditCartViewModel
    {
        public AutoOrderAddEditCartViewModel()
        {
            ProductsList = new List<Item>();
        }

        public AutoOrder AutoOrder { get; set; }
        public List<Item> ProductsList { get; set; }
    }
}