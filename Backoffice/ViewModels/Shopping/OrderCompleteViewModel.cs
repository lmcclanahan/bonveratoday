using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Backoffice.ViewModels
{
    public class OrderCompleteViewModel
    {
        public int OrderID { get; set; }
        public int AutoOrderID { get; set; }
        public ExigoService.Order Order { get; set; }
        public int WillCallShipMethodID { get; set; }
    }
}