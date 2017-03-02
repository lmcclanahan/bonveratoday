using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class OrderCompleteViewModel
    {
        public int OrderID { get; set; }

        public int WillCallShipMethodID { get; set; }
        public ExigoService.Order Order { get; set; }
    }
}