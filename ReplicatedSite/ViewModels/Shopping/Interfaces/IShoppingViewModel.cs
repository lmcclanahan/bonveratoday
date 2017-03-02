using ExigoService;
using ReplicatedSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public interface IShoppingViewModel
    {
        ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        string[] Errors { get; set; }
    }
}