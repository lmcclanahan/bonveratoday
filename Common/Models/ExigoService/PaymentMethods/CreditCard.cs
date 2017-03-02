using Common;
using Common.Api.ExigoWebService;
using Common.Filters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ExigoService
{
    public class CreditCard : ICreditCard
    {
        public CreditCard()
        {
            this.Type = CreditCardType.New;
            this.BillingAddress = new Address();
            this.ExpirationMonth = DateTime.Now.Month;
            this.ExpirationYear = DateTime.Now.Year;
            this.AutoOrderIDs = new int[0];
        }
        public CreditCard(CreditCardType type)
        {
            this.Type = type;
            this.BillingAddress = new Address();
            this.ExpirationMonth = DateTime.Now.Month;
            this.ExpirationYear = DateTime.Now.Year;
        }

        public CreditCardType Type { get; set; }

        [Display(Name = "Name on Card")]
        [Required]
        public string NameOnCard { get; set; }

        [Display(Name = "Card Number")]
        [Required, Luhn(ErrorMessage = "Invalid Card Number"), MinLength(12, ErrorMessage="Card Number Too Short")]
        public string CardNumber { get; set; }

        [Display(Name = "Expiration Month")]
        [Required]
        public int ExpirationMonth { get; set; }

        [Display(Name = "Expiration Year")]
        [Required]
        public int ExpirationYear { get; set; }

        public string GetToken()
        {
            if (!IsComplete) return string.Empty;

            return Exigo.Payments().FetchCreditCardToken(
                this.CardNumber,
                this.ExpirationMonth,
                this.ExpirationYear);
        }

        [Required, Display(Name = "CVV"), RegularExpression(@"^\d{3,4}$", ErrorMessage="Please enter a Valid CVV")]
        public string CVV { get; set; }

        [Required, DataType("Address")]
        public Address BillingAddress { get; set; }

        public int[] AutoOrderIDs { get; set; }

        public DateTime ExpirationDate
        {
            get { return new DateTime(this.ExpirationYear, this.ExpirationMonth, DateTime.DaysInMonth(this.ExpirationYear, this.ExpirationMonth)); }
        }

        public bool IsExpired
        {
            get { return this.ExpirationDate < DateTime.Now; }
        }
        public bool IsComplete
        {
            get
            {
                if (string.IsNullOrEmpty(NameOnCard)) return false;
                if (string.IsNullOrEmpty(CardNumber)) return false;
                if (ExpirationMonth == 0) return false;
                if (ExpirationYear == 0) return false;
                if (!BillingAddress.IsComplete) return false;

                return true;
            }
        }
        public bool IsValid
        {
            get
            {
                if (!IsComplete) return false;
                if (IsExpired) return false;
                if (!IsTestCreditCard && !GlobalUtilities.ValidateCreditCard(CardNumber)) return false;

                return true;
            }
        }
        public bool IsUsedInAutoOrders
        {
            get { return this.AutoOrderIDs.Length > 0; }
        }
        public bool IsTestCreditCard
        {
            get { return this.CardNumber == "9696" || this.CardNumber == "9696969696969696"; }
        }

        public AutoOrderPaymentType AutoOrderPaymentType
        {
            get
            {
                switch(this.Type)
                {
                    case CreditCardType.Primary:
                    default: return Common.Api.ExigoWebService.AutoOrderPaymentType.PrimaryCreditCard;

                    case CreditCardType.Secondary: return Common.Api.ExigoWebService.AutoOrderPaymentType.SecondaryCreditCard;
                }
            }
        }
    }
}