using Common;
using Common.Helpers;
using Common.HtmlHelpers;
using ExigoService;
using ReplicatedSite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class CustomerSiteViewModel
    {
        public int CustomerID { get; set; }
        public CustomerSite ClientSite { get; set; }
        public string AvatarUrl { get; set; }
        public Customer Client { get; set; }
        public string WebAlias { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; }

        
        public string LoginName { get; set; }

        public int LanguageID { get; set; }

        [Display(Name = "Phone Number")]
        public string PrimaryPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Fax { get; set; }
        public IEnumerable<Address> Addresses { get; set; }

        public bool IsOptedIn { get; set; }


        public IEnumerable<Language> Languages { get; set; }
    }
}