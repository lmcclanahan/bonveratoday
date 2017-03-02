using Common;
using ExigoService;
using Common.Api.ExigoWebService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Backoffice.ViewModels
{
    public class AccountOverviewViewModel
    {
        public AccountOverviewViewModel()
        {
            this.CustomerSite = new CustomerSite();
            this.CustomerSite.Address = new Address();
        }

        public int CustomerID { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, System.Web.Mvc.Remote("IsValidWebAlias", "Account")]
        public string WebAlias { get; set; }

        [Required, System.Web.Mvc.Remote("IsValidLoginName", "Account"), RegularExpression(GlobalSettings.RegularExpressions.LoginName, ErrorMessage = "Please enter a user name that does not contain special characters")]
        public string LoginName { get; set; }

        
        public string TeamPlacementPreferenceID { get; set; }
        public string TeamPlacementPreference { get; set; }

        [Display(Name = "SSN"), DataType("TaxID")]
        [RegularExpression(GlobalSettings.RegularExpressions.TaxID, ErrorMessage = "Invalid SSN")]
        [Remote("IsTaxIDAvailable_Account", "App", ErrorMessage = "Tax ID already in the system. If this is your Tax ID and you feel this is a mistake, please contact customer service.")]
        public string TaxID { get; set; }
        public TaxIDType TaxIDType { get; set; }
        public bool TaxIDIsSet { get; set; }
        public string MaskedTaxID { get; set; }
        
        public List<System.Web.Mvc.SelectListItem> TeamPlacementPreferenceOptions { get; set; }

        [Required]
        public string Password { get; set; }
        [Required, System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Your passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public int LanguageID { get; set; }
        public int AutoOrderID { get; set; }

        [Display(Name = "Primary Phone")]
        public string PrimaryPhone { get; set; }
        [Display(Name = "Secondary Phone")]
        public string SecondaryPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Fax { get; set; }
        public IEnumerable<Address> Addresses { get; set; }

        public bool IsOptedIn { get; set; }

        public CustomerSite CustomerSite { get; set; }

        public Customer Enroller { get; set; }
        public Customer Sponsor { get; set; }
        public IEnumerable<Language> Languages { get; set; }

        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string YouTubeUrl { get; set; }
        public string BlogUrl { get; set; }

        public AutoOrder Membership { get; set; }
        public string ActiveMembership { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}