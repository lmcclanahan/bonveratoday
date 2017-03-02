using Backoffice.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backoffice.ViewModels
{
    public class ItemDetailViewModel : IShoppingListViewModel
    {
        public Item Item { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }
        public IOrderConfiguration OrderConfiguration { get; set; }

        // IShoppingListViewModel properties, used for category list
        public string parentCategoryKey { get; set; }
        public string subCategoryKey { get; set; }
        public ItemCategory CurrentCategory { get; set; }
        public int CategoryID { get; set; }
        public IEnumerable<ItemCategory> Categories { get; set; }
    }
}