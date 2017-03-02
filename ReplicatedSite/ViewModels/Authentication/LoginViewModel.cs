using System.ComponentModel.DataAnnotations;

namespace ReplicatedSite.ViewModels
{
    public class LoginViewModel
    {
        [Required,Display(Name = "Username")]
        public string LoginName { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Password")]
        public string Password { get; set; }

        public bool IsSmartShopperRegistration { get; set; }
    }
}