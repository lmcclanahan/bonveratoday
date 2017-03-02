using Backoffice.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backoffice.ViewModels
{
    public class CategoryViewModel
    {
        public CategoryViewModel()
        {
            Categories = new List<string>();
        }

        public IItemCategory MainCategory { get; set; }

        public List<string> Categories { get; set; }
    }
}