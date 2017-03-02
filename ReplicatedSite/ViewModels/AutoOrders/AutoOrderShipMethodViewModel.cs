using ExigoService;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels.AutoOrders
{
    public class AutoOrderShipMethodViewModel
    {
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public IEnumerable<IShipMethod> AkHiPrShipMethods { get; set; }
        public int AutoorderID { get; set; }
        public string Error { get; set; }
    }
}