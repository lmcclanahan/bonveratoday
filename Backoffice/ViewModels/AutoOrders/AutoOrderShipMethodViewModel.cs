using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels.AutoOrders
{
    public class AutoOrderShipMethodViewModel
    {
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public int AutoorderID { get; set; }
        public string Error { get; set; }
    }
}