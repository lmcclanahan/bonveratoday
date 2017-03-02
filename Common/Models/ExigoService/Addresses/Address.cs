using Common.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ExigoService
{
    public class Address : IAddress
    {
        public Address()
        {
            this.AddressType = AddressType.New;
        }

        [Required]
        public AddressType AddressType { get; set; }

        [Required]
        [Display(Name = "Street Address")]
        public virtual string Address1 { get; set; }
        public string Address2 { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        //Added validation for zip code. Ticket#79866 8/26/2016 Lindsey A.
        //20170118 828245 DV. Unfortunately since you cannot dynamically set this data annotation we'll have to resort back to client-side validation.  Also as of this writing the validation behavior varies
        //between ShippingAddress.cshtml and Address.cshtml per client request.
        //[RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessageResourceType = typeof(Resources.Common), ErrorMessageResourceName = "InvalidZipCode")]
        public string Zip { get; set; }
        [Required]
        public string Country { get; set; }

        public string AddressDisplay
        {
            get { return this.Address1 + ((!this.Address2.IsEmpty()) ? " {0}".FormatWith(this.Address2) : ""); }
        }
        public bool IsComplete
        {
            get
            {
                return
                    !string.IsNullOrEmpty(Address1) &&
                    !string.IsNullOrEmpty(City) &&
                    !string.IsNullOrEmpty(State) &&
                    !string.IsNullOrEmpty(Zip) &&
                    !string.IsNullOrEmpty(Country);
            }
        }
        public bool IsNotPoBox
        {
           
            get
            {
                return (string.IsNullOrEmpty(Address1)) ? false: Regex.IsMatch(Address1, @"^(?!(?:[pP](?:ost)?\.?\s*[oO0](?:ffice)?\.?\s*[bB](?:[oO0][xX])?|[bB][oO0][xX])).*$");
                    
            }
        }
        public string GetHash()
        {
            return Security.GetHashString(string.Format("{0}|{1}|{2}|{3}|{4}",
                this.AddressDisplay.Trim(),
                this.City.Trim(),
                this.State.Trim(),
                this.Zip.Trim(),
                this.Country.Trim()));
        }
        public override bool Equals(object obj)
        {
            try
            {
                var hasha = this.GetHash();
                var hashb = ((Address)obj).GetHash();
                return hasha.Equals(hashb);
            }
            catch
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}