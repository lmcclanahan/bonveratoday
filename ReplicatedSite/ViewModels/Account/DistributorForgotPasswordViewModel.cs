using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class DistributorForgotPasswordViewModel
    {
        [Required, Display(Name = "Email")]
        [DataType(DataType.Text, ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }

        public int CustomerID { get; set; }
    }
}