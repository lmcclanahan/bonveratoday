using ExigoService;

namespace Backoffice.ViewModels.AutoOrders
{
    public class AutoOrderShippingAddressViewModel
    {
        public int AutoorderID { get; set; }
        public Address ShippingAddress { get; set; }
    }
}