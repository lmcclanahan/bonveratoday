using ExigoService;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels.AutoOrders
{
    public class AutoOrderAddEditCartViewModel
    {
        
        public AutoOrder AutoOrder { get; set; }

        public List<Item> ProductsList { get; set; }

        public string Products { get; set; }
    }
}