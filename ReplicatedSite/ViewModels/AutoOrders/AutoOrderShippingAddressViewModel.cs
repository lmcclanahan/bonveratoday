using ExigoService;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels
{
    public class AutoOrderShippingAddressViewModel
    {
        public IEnumerable<ShippingAddress> Addresses { get; set; }
    }
}