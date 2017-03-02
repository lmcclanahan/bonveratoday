using ReplicatedSite.Models;
using Common.Api.ExigoWebService;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class AutoOrderSettingsViewModel : IShoppingViewModel
    {
        public DateTime AutoOrderStartDate { get; set; }
        public FrequencyType AutoOrderFrequencyType { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }
    }
}