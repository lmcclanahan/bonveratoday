using ReplicatedSite.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class ShippingAddressesViewModel : IShoppingViewModel
    {
        public IEnumerable<ShippingAddress> Addresses { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public bool SaveAddress { get; set; }
        public string[] Errors { get; set; }
    }
}