using Common;
using Common.Api.ExigoOData;
using Common.Services;
using ReplicatedSite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.Models
{
    public class DistributorSearch
    {
        [Display(Name = "Website Address"), DataType(DataType.Text), Required(ErrorMessage = "You Must Enter a Distributor's Site Name to Search by Website Address"), RegularExpression(GlobalSettings.RegularExpressions.LoginName)]
        public string WebAlias { get; set; }

        [Display(Name = "My Distributor's ID Is:"), DataType(DataType.Text), Required]
        public int DistributorID { get; set; }

        [Display(Name = "The Event ID Is:"), DataType(DataType.Text), Required]
        public int EventCode { get; set; }

        [Display(Name = "Distributor's First Name:*"), DataType(DataType.Text), Required(ErrorMessage = "Distributor's First Name is required")]
        public string DistributorFirstName { get; set; }
        [Display(Name = "Distributor's Last Name:*"), DataType(DataType.Text), Required(ErrorMessage = "Distributor's Last Name is required")]
        public string DistributorLastName { get; set; }
        [Display(Name = "Desiger's State:*"), Required(ErrorMessage = "Distributor's State is required")]
        public string DistributorState { get; set; }
        [Display(Name = "City: (optional)")]
        public string DistributorCity { get; set; }
        [Display(Name = "Zip Code: (optional)"), DataType(DataType.PostalCode)]
        public string DistributorZipCode { get; set; }
        public string Country { get; set; }
        public string AvatarUrl { get; set; }

        [Display(Name = "My Zip Code is:"), DataType(DataType.PostalCode), Required(ErrorMessage = "You Must Enter a Valid Zip Code to Search by Zip Code")]
        public string CustomerZipCode { get; set; }

        public SearchType SearchTypeID { get; set; }

        public List<CustomerSite> BaseQuery { get; set; }

    }
}