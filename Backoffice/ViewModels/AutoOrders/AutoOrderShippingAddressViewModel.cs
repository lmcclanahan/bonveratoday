using ExigoService;
using System.Collections.Generic;

namespace Backoffice.ViewModels
{
    public class AutoOrderShippingAddressViewModel
    {
        public IEnumerable<ShippingAddress> Addresses { get; set; }
    }
}