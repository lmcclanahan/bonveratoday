using ExigoService;
using ReplicatedSite.Models;
using System.Collections.Generic;

namespace ReplicatedSite.ViewModels
{
    public class EnrollmentReviewViewModel : IEnrollmentViewModel
    {       
        public IEnumerable<IItem> Items { get; set; }
        public OrderCalculationResponse Totals { get; set; }
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public int ShipMethodID { get; set; }
        public EnrollmentPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }

        public decimal Discount { get; set; }
        public int WillCallShipMethodID { get; set; }

        public Customer Enroller { get; set; }
        public Customer Sponsor { get; set; }
    }
}