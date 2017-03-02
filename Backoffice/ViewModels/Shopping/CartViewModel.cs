using Backoffice.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Backoffice.ViewModels
{
    public class CartViewModel : IShoppingViewModel
    {
        public IEnumerable<IItem> Items { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }

        public bool HasActiveAutoOrder { get; set; }
        public int AutoOrderCount { get; set; }
    }
}