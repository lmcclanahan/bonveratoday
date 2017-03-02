using System;
using ExigoService;
using Common.Api.ExigoWebService;
using System.Collections.Generic;

namespace Backoffice.Models
{
    public class ShoppingCartCheckoutPropertyBag : BasePropertyBag
    {
        private string version = "3.0.0";
        private int expires    = 15;



        #region Constructors
        public ShoppingCartCheckoutPropertyBag()
        {
            this.CustomerID = Identity.Current.CustomerID;
            this.Customer = new Customer();
            this.Expires = expires;

            if (string.IsNullOrEmpty(this.ShippingDiscountID)) this.ShippingDiscountID = "0";
        }
        #endregion

        #region Properties
        public int CustomerID { get; set; }
        public Customer Customer { get; set; }

        public IEnumerable<ShippingAddress> Addresses { get; set; }

        public ShippingAddress ShippingAddress { get; set; }
        public ShippingAddress BillingAddress { get; set; }
        public bool BillingSameAsShipping { get; set; }

        public ShippingAddress AutoOrderShippingAddress { get; set; }
        public ShippingAddress AutoOrderBillingAddress { get; set; }
        public bool AutoOrderBillingSameAsShipping { get; set; }

        public DateTime AutoOrderStartDate { get; set; }
        public FrequencyType AutoOrderFrequencyType { get; set; }

        public int ShipMethodID { get; set; }
        public int AutoOrderShipMethodID { get; set; }

        public IPaymentMethod PaymentMethod { get; set; }
        public IPaymentMethod AutoOrderPaymentMethod { get; set; }

        public string ShippingDiscountID { get; set; }

        public bool HasActiveAutoOrder { get; set; }
        #endregion

        #region Methods
        public override T OnBeforeUpdate<T>(T propertyBag)
        {
            propertyBag.Version = version;

            return propertyBag;
        }
        public override bool IsValid()
        {
            var currentCustomerID = Identity.Current.CustomerID;
            return this.Version == version && this.CustomerID == currentCustomerID;
        }
        #endregion
    }
}