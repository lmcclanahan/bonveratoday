using ExigoService;
using System.Collections.Generic;
using System.Linq;

namespace ReplicatedSite.ViewModels
{
    public class EnrollmentSummaryViewModel
    {
        public IEnumerable<Item> OrderItems { get; set; }
        public IEnumerable<Item> AutoOrderItems { get; set; }
        public IEnumerable<Item> OrderEnrollmentPacks { get; set; }
        public IEnumerable<Item> EnrollmentAutoOrderPack { get; set;}  

        public decimal OrderSubtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal Shipping { get; set; }
        public decimal Discount { get; set; }

        public decimal AutoOrderSubtotal { get; set; }
        public decimal AutoOrderTax { get; set; }
        public decimal AutoOrderTotal { get; set; }
        public decimal AutoOrderShipping { get; set; }
        public decimal AutoOrderDiscount { get; set; }

        public decimal EnrollmentPackSubtotal { get; set; }
        public decimal EnrollmentPackTax { get; set; }
        public decimal EnrollmentPackTotal { get; set; }
        public decimal EnrollmentPackShipping { get; set; }
        public decimal EnrollmentPackDiscount { get; set; }

        public bool HideShippingAndTax { get; set; }

        public bool HasInitialOrderItems
        {
            get { return this.OrderItems != null && this.OrderItems.Count() > 0; }
        }
        public bool HasOrderEnrollmentPacks
        {
            get { return this.OrderEnrollmentPacks != null && this.OrderEnrollmentPacks.Count() > 0; }
        }
        public bool HasNonPackAutoOrderItems
        {
            get { return this.AutoOrderItems != null && this.AutoOrderItems.Count() > 0; }
        }
        public bool HasPackAutoOrderItems
        {
            get { return this.EnrollmentAutoOrderPack != null && this.EnrollmentAutoOrderPack.Count() > 0; }
        }

        public bool HasOrderItems
        {
            get { return this.HasInitialOrderItems || this.HasOrderEnrollmentPacks; }
        }
        public bool HasAutoOrderItems
        {
            get { return this.HasPackAutoOrderItems || this.HasNonPackAutoOrderItems; }
        }

        public bool HasItems
        {
            get { return this.HasOrderItems || this.HasAutoOrderItems; }
        }

    }
}