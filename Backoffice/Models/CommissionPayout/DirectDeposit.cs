using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExigoService;
using System.ComponentModel.DataAnnotations;
using Common.Api.ExigoWebService;

namespace Backoffice.Models.CommissionPayout
{
    public class CommissionPayout
    {
        [Required(ErrorMessage = "Name on Account is required")]
        public string NameOnAccount { get; set; }
        public string BankName { get; set; }
        [Required(ErrorMessage = "Account Number is required")]
        [MinLength(5, ErrorMessage = "Account Number must be at least 5 numbers long")]
        public string AccountNumber { get; set; }
        [Required(ErrorMessage = "Routing Number is required")]
        [MinLength(5, ErrorMessage = "Routing Number must be at least 5 numbers long")]
        public string RoutingNumber { get; set; }
        public DepositAccountType DepositAccountTypeForm { get; set; }
        public bool IsComplete
        {
            get
            {
                if (string.IsNullOrEmpty(NameOnAccount)) return false;
                if (string.IsNullOrEmpty(AccountNumber)) return false;
                if (string.IsNullOrEmpty(RoutingNumber)) return false;

                return true;
            }
        }
    }
}