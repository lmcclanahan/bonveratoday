using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Common;
using ExigoService;
using ReplicatedSite.Models;

namespace ReplicatedSite.ViewModels
{
    public class AccountRegistrationViewModel
    {
        public AccountRegistrationViewModel()
        {
            this.CalculatedOrder = new OrderCalculationResponse();
        }

        [Required, Display(Name="First Name:"), DataType(DataType.Text)]
        public string FirstName { get; set; }
        [Display(Name = "Middle Name:"), DataType(DataType.Text)]
        public string MiddleName { get; set; }
        [Required, Display(Name = "Last Name:"), DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required, Display(Name = "Email / Username:"), DataType(DataType.EmailAddress), RegularExpression(GlobalSettings.RegularExpressions.EmailAddresses, ErrorMessage = "Incorrect Email Format, Please Re-enter Email.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [System.Web.Mvc.Remote("IsUserNameAvailable", "App", ErrorMessage = "This email already exists in our records - try another one.")]
        public string Username { get; set; }

        [Display(Name = "Confirm Email:"), DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid Email Address"), Compare("Username", ErrorMessage = "The Email Addresses you entered do not match, please try again.")]
        public string ConfirmEmail { get; set; }

        public bool IsOptedIn { get; set; }

        [Display(Name = "Join as a Preferred Customer")]
        public bool JoinAsPreferred { get; set; }

        [Display(Name = "Password:"), Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm Password:")]
        [Required, Compare("Password", ErrorMessage = "The passwords you entered do not match, please try again."), DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public bool IsOrphan { get; set; }

        public int EnrollerID { get; set; }
        public bool ReturnedError { get; set; }

        public ShoppingCartItemsPropertyBag ShoppingCart { get; set; }
        
        // Smart Shopper Registration 
        public CreditCard CreditCard { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public bool HasAutoOrderItems { get; set; }
        public Item SmartShopperSubscriptionItem { get; set; }
        public Item FirstOrderPack { get; set; }
        public SmartShopperOption SmartShopperOption { get; set; }
        public OrderCalculationResponse CalculatedOrder { get; set; }
        public int ShipMethodID { get; set; }

        // The selected Smart Shopper Pack option
        public string SmartShopperItemCode { get; set; }

        // Calculated discount based on items that come back from the calculated order that have a value of less than 0 - Mike M.
        public decimal Discount { get; set; }

        // Will Call
        public int WillCallShipMethodID { get; set; }
    }

    public enum SmartShopperOption
    {
        CreateReplenishment,
        PurchaseSubscription
    }
}