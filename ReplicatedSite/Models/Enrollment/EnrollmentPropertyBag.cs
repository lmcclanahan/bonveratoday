using Common;
using ExigoService;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReplicatedSite.Models
{
    public class EnrollmentPropertyBag : BasePropertyBag
    {
        private string version = "2.3.0";
        private int expires = 10;

        #region Constructors
        public EnrollmentPropertyBag()
        {
            this.Expires = expires;

            this.Customer = this.Customer ?? new Customer();
            this.DirectDeposit = this.DirectDeposit ?? new BankAccount();
        }
        #endregion

        #region Properties
        [Required]
        public Customer Customer { get; set; }
        public bool ShowBirthday { get; set; }
        public bool HasChosenNoBirthday { get; set; }
        public BankAccount DirectDeposit { get; set; }

        public int EnrollerID { get; set; }
        public int SponsorID { get; set; }
        public EnrollmentType EnrollmentType { get; set; }
        public MarketName SelectedMarket { get; set; }

        public ShippingAddress ShippingAddress { get; set; }

        public int PaymentTypeID { get; set; }
        public IPaymentMethod PaymentMethod { get; set; }

        [Required]
        public int ShipMethodID { get; set; }
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public bool UseSameShippingAddress { get; set; }
        public bool UseSameBillingAddress { get; set; }
        public bool WantAnnualRenewal { get; set; }
        public bool WantICAAMembership { get; set; }

        // Flag to determine if this is a Back Office user flow
        public bool IsBackofficeEnrollment { get; set; }
        #endregion

        #region Methods
        public override T OnBeforeUpdate<T>(T propertyBag)
        {
            propertyBag.Version = version;

            return propertyBag;
        }
        public override bool IsValid()
        {
            return this.Version == version;
        }
        #endregion
    }
}