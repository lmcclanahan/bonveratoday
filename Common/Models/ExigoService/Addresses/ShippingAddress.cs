using Common;
using System.ComponentModel.DataAnnotations;

namespace ExigoService
{
    public class ShippingAddress : Address
    {
        public ShippingAddress() { }
        public ShippingAddress(Address address)
        {
            base.AddressType = address.AddressType;
            base.Address1 = address.Address1;
            base.Address2 = address.Address2;
            base.City = address.City;
            base.State = address.State;
            base.Zip = address.Zip;
            base.Country = address.Country;
        }
        [Required]
        [Display(Name = "Street Address")]
        [RegularExpression(@"^(?!(?:[pP](?:ost)?\.?\s*[oO0](?:ffice)?\.?\s*[bB](?:[oO0][xX])?|[bB][oO0][xX])).*$", ErrorMessage = "We are sorry, we are unable to ship to PO Boxes")]
        public override string Address1 { get { return base.Address1; } set { base.Address1 = value; } }
        [Required(ErrorMessage = "First Name is required"), Display(Name = "First Name")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Last Name is required"), Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string Company { get; set; }
        [DataType(DataType.PhoneNumber), Display(Name = "Phone Number")]
        public string Phone { get; set; }
        [Required, DataType(DataType.EmailAddress), RegularExpression(GlobalSettings.RegularExpressions.EmailAddresses, ErrorMessage = "This email doesn't look right - can you check it again?"), Display(Name = "Email")]
        public string Email { get; set; }

        public string FullName
        {
            get { return string.Join(" ", this.FirstName, this.LastName); }
        }
    }
}