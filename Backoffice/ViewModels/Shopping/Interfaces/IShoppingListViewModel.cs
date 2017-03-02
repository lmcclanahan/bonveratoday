using ExigoService;
using Backoffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backoffice.ViewModels
{
    public interface IShoppingListViewModel : IShoppingViewModel
    {
        string parentCategoryKey { get; set; }
        string subCategoryKey { get; set; }
        ItemCategory CurrentCategory { get; set; }
        int CategoryID { get; set; }
        IEnumerable<ItemCategory> Categories { get; set; }
    }
}