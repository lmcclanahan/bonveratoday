using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReplicatedSite.ViewModels
{
    // Used for Autoship Pop up modal
    public class AutoOrderCartReviewViewModel
    {
        public AutoOrder ActiveAutoOrder { get; set; }
        public IEnumerable<Item> AutoOrderCartItems { get; set; }
        public OrderCalculationResponse CalculatedAutoOrder { get; set; }
        public string AutoOrderFrequencyType { get; set; }
        public decimal AvailablePoints { get; set; }

        public int SelectedDay { get; set; }

        public DateTime NextRunDate { get; set; }

        public List<SelectListItem> AvailableRunDays
        {
            get
            {
                var list = new List<SelectListItem>();


                for (int i = 1; i < 29; i++)
                {
                    list.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                }

                return list;
            }
        }
    }
}