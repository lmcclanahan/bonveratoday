using Backoffice.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backoffice.ViewModels
{
    public class ItemListViewModel2 : IShoppingViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<IItem> OrderItems { get; set; }
        public IEnumerable<IItem> AutoOrderItems { get; set; }

        public int Page { get; set; }
        public int RecordCount { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }
    }
}