using Backoffice.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backoffice.ViewModels
{
    public class ItemListViewModel : IShoppingListViewModel
    {
        public ItemListViewModel()
        {
            Cart = new CartViewModel();
        }


        public List<Item> Items { get; set; }
        public CartViewModel Cart { get; set; }
        public IOrderConfiguration OrderConfiguration { get; set; }
        public int Page { get; set; }
        public int RecordCount { get; set; }
        public string CategoryContentKey { get; set; }
        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }
        public bool HasAutoOrderItems { get; set; }


        // IShoppingListViewModel properties, used for category list
        public string parentCategoryKey { get; set; }
        public string subCategoryKey { get; set; }
        public ItemCategory CurrentCategory { get; set; }
        public int CategoryID { get; set; }
        public IEnumerable<ItemCategory> Categories { get; set; }

    }
}