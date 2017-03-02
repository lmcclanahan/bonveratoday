using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReplicatedSite.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public int CustomerID { get; set; }

        [Required]
        public int CustomerType { get; set; }

        [Required, Display(Name = "New Password")]
        public string Password { get; set; }

        [Required, Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public bool IsExpiredLink { get; set; }
    }
}